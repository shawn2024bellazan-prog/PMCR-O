"""Tests for the SkillLibrary loader. These load the real skills/ directory."""

from __future__ import annotations

from pathlib import Path

import pytest

from reasoning_skills import (
    SkillError,
    SkillLibrary,
    SkillNotFoundError,
    SkillParseError,
    parse_skill_text,
)

SKILLS_DIR = Path(__file__).resolve().parent.parent / "skills"

EXPECTED = {
    "adaptive-thinking-budget",
    "chain-of-thought",
    "chain-of-verification",
    "least-to-most",
    "meta-prompting",
    "multi-agent-debate",
    "plan-and-solve",
    "program-aided-reasoning",
    "react",
    "reflection-refinement",
    "retrieval-augmented-reasoning",
    "self-consistency",
    "socratic-questioning",
    "structured-output",
    "tool-augmented-reasoning",
    "tree-of-thought",
    "verifier-best-of-n",
}

REQUIRED_SECTIONS = [
    "## What this skill does",
    "## Instructions",
    "## Template",
    "## Security notes",
    "## Examples",
    "## Limitations",
]


@pytest.fixture(scope="module")
def library() -> SkillLibrary:
    return SkillLibrary.from_directory(SKILLS_DIR)


def test_loads_all_skills(library):
    assert {s.name for s in library.list()} == EXPECTED
    assert len(library) == len(EXPECTED)


def test_list_is_sorted(library):
    names = [s.name for s in library.list()]
    assert names == sorted(names)


def test_get_returns_skill(library):
    skill = library.get("react")
    assert skill.name == "react"
    assert skill.path.name == "SKILL.md"
    assert "react" in library


def test_get_unknown_raises(library):
    with pytest.raises(SkillNotFoundError) as info:
        library.get("does-not-exist")
    assert "does-not-exist" in str(info.value)


def test_not_found_is_also_keyerror_and_skillerror(library):
    with pytest.raises(KeyError):
        library.get("nope")
    with pytest.raises(SkillError):
        library.get("nope")


def test_search_by_tag(library):
    names = {s.name for s in library.search(tag="verification")}
    assert names == {
        "chain-of-verification",
        "self-consistency",
        "tool-augmented-reasoning",
        "verifier-best-of-n",
    }


def test_search_by_status(library):
    stable = {s.name for s in library.search(status="stable")}
    assert stable == {"chain-of-thought", "tree-of-thought"}


def test_search_by_status_enum_and_tag_combined(library):
    from reasoning_skills import SkillStatus

    names = {s.name for s in library.search(tag="reasoning", status=SkillStatus.STABLE)}
    assert names == {"chain-of-thought", "tree-of-thought"}
    assert library.search(tag="reasoning", status="deprecated") == []


def test_search_no_filters_returns_all(library):
    assert len(library.search()) == len(library)


def test_search_unknown_tag_returns_empty(library):
    assert library.search(tag="no-such-tag") == []


def test_search_bad_status_raises(library):
    with pytest.raises(ValueError):
        library.search(status="finished")


@pytest.mark.parametrize("name", sorted(EXPECTED))
def test_every_skill_has_required_sections(library, name):
    body = library.get(name).body
    for section in REQUIRED_SECTIONS:
        assert section in body, f"{name} is missing {section!r}"


@pytest.mark.parametrize("name", sorted(EXPECTED))
def test_every_template_renders_with_required_inputs(library, name):
    skill = library.get(name)
    assert skill.template_source, f"{name} has no jinja2 block"
    values = {}
    for param in skill.frontmatter.inputs:
        if not param.required:
            continue
        if param.type.value == "array":
            values[param.name] = [{"id": "p1", "text": "t", "input": "i", "expected": "e"}] if name in {
                "retrieval-augmented-reasoning", "meta-prompting"
            } else ["item"]
        elif param.type.value == "object":
            values[param.name] = {"type": "object"}
        elif param.type.value == "integer":
            values[param.name] = 3
        else:
            values[param.name] = "sample text"
    text = skill.render(**values)
    assert text.strip()


@pytest.mark.parametrize("name", sorted(EXPECTED))
def test_untrusted_inputs_are_xml_tagged(library, name):
    skill = library.get(name)
    source = skill.template_source
    for param in skill.frontmatter.inputs:
        if "untrusted" in param.description.lower():
            assert f"<{param.name}>" in source, f"{name}: {param.name} is not wrapped in an XML tag"


@pytest.mark.parametrize("name", sorted(EXPECTED))
def test_skill_name_matches_directory(library, name):
    assert library.get(name).path.parent.name == name


def test_render_leaves_markup_unescaped(library):
    text = library.get("chain-of-thought").render(task="a < b & c", context="x")
    assert "a < b & c" in text


def test_render_missing_variable_raises(library):
    with pytest.raises(Exception):
        library.get("chain-of-thought").render()


def test_from_directory_rejects_missing_dir(tmp_path):
    with pytest.raises(SkillError):
        SkillLibrary.from_directory(tmp_path / "missing")


def test_from_directory_rejects_name_directory_mismatch(tmp_path):
    good = (SKILLS_DIR / "react" / "SKILL.md").read_text(encoding="utf-8")
    folder = tmp_path / "wrong-name"
    folder.mkdir()
    (folder / "SKILL.md").write_text(good, encoding="utf-8")
    with pytest.raises(SkillParseError):
        SkillLibrary.from_directory(tmp_path)


def test_parse_rejects_missing_frontmatter():
    with pytest.raises(SkillParseError):
        parse_skill_text("# no frontmatter", Path("x/SKILL.md"))


def test_parse_rejects_invalid_yaml():
    with pytest.raises(SkillParseError):
        parse_skill_text("---\nname: [unclosed\n---\nbody", Path("x/SKILL.md"))


def test_parse_rejects_invalid_frontmatter_fields():
    with pytest.raises(SkillParseError):
        parse_skill_text("---\nname: Bad_Name\n---\nbody", Path("x/SKILL.md"))
