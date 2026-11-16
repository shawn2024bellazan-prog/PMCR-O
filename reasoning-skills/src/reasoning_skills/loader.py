"""Load, validate, and query SKILL.md files."""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Iterator, List, Optional, Union

import yaml
from jinja2 import StrictUndefined
from jinja2.sandbox import SandboxedEnvironment
from pydantic import ValidationError

from .schema import SkillFrontmatter, SkillStatus

_FRONTMATTER = re.compile(r"\A---\s*\n(.*?)\n---\s*\n?(.*)\Z", re.DOTALL)
_TEMPLATE_BLOCK = re.compile(r"```jinja2\n(.*?)\n```", re.DOTALL)


class SkillError(Exception):
    """Base class for loader errors."""


class SkillNotFoundError(SkillError, KeyError):
    """Raised when a skill name is not in the library."""

    def __str__(self) -> str:  # KeyError would otherwise repr() the message
        return str(self.args[0]) if self.args else ""


class SkillParseError(SkillError):
    """Raised when a SKILL.md file cannot be parsed or fails validation."""


@dataclass(frozen=True)
class Skill:
    """A parsed and validated skill."""

    frontmatter: SkillFrontmatter
    body: str
    path: Path

    @property
    def name(self) -> str:
        return self.frontmatter.name

    @property
    def template_source(self) -> Optional[str]:
        """The Jinja2 template from the skill body, or None if absent."""
        match = _TEMPLATE_BLOCK.search(self.body)
        return match.group(1) if match else None

    def render(self, **variables: Any) -> str:
        """Render the skill's Jinja2 template in a sandbox.

        Undefined variables raise instead of rendering as empty text.
        """
        source = self.template_source
        if source is None:
            raise SkillError(f"skill {self.name!r} has no jinja2 template block")
        env = SandboxedEnvironment(undefined=StrictUndefined, autoescape=False, keep_trailing_newline=True)
        return env.from_string(source).render(**variables)


def parse_skill_text(text: str, path: Path) -> Skill:
    """Parse SKILL.md text into a Skill."""
    match = _FRONTMATTER.match(text)
    if not match:
        raise SkillParseError(f"{path}: missing YAML frontmatter delimited by '---'")
    raw, body = match.groups()
    try:
        data = yaml.safe_load(raw)
    except yaml.YAMLError as exc:
        raise SkillParseError(f"{path}: invalid YAML frontmatter: {exc}") from exc
    if not isinstance(data, dict):
        raise SkillParseError(f"{path}: frontmatter must be a mapping")
    try:
        frontmatter = SkillFrontmatter.model_validate(data)
    except ValidationError as exc:
        raise SkillParseError(f"{path}: invalid frontmatter:\n{exc}") from exc
    return Skill(frontmatter=frontmatter, body=body, path=path)


class SkillLibrary:
    """An in-memory collection of validated skills."""

    def __init__(self, skills: Optional[List[Skill]] = None) -> None:
        self._skills: Dict[str, Skill] = {}
        for skill in skills or []:
            if skill.name in self._skills:
                raise SkillParseError(f"duplicate skill name: {skill.name!r}")
            self._skills[skill.name] = skill

    @classmethod
    def from_directory(cls, directory: Union[str, Path]) -> "SkillLibrary":
        """Load every ``<directory>/<skill>/SKILL.md``.

        The frontmatter ``name`` must match the skill's directory name.
        """
        root = Path(directory)
        if not root.is_dir():
            raise SkillError(f"not a directory: {root}")
        skills: List[Skill] = []
        for skill_file in sorted(root.glob("*/SKILL.md")):
            skill = parse_skill_text(skill_file.read_text(encoding="utf-8"), skill_file)
            if skill.name != skill_file.parent.name:
                raise SkillParseError(
                    f"{skill_file}: name {skill.name!r} does not match directory {skill_file.parent.name!r}"
                )
            skills.append(skill)
        return cls(skills)

    def get(self, name: str) -> Skill:
        try:
            return self._skills[name]
        except KeyError:
            known = ", ".join(sorted(self._skills)) or "(none)"
            raise SkillNotFoundError(f"skill {name!r} not found; known skills: {known}") from None

    def list(self) -> List[Skill]:
        return [self._skills[name] for name in sorted(self._skills)]

    def search(
        self,
        tag: Optional[str] = None,
        status: Optional[Union[str, SkillStatus]] = None,
    ) -> List[Skill]:
        """Return skills matching all given filters (AND). No filters returns all."""
        wanted_status = SkillStatus(status) if status is not None else None
        results = []
        for skill in self.list():
            if tag is not None and tag not in skill.frontmatter.tags:
                continue
            if wanted_status is not None and skill.frontmatter.status != wanted_status:
                continue
            results.append(skill)
        return results

    def __len__(self) -> int:
        return len(self._skills)

    def __iter__(self) -> Iterator[Skill]:
        return iter(self.list())

    def __contains__(self, name: object) -> bool:
        return name in self._skills
