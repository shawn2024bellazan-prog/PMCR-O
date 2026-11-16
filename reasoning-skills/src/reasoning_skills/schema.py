"""Pydantic v2 model for SKILL.md frontmatter validation."""

from __future__ import annotations

import re
from enum import Enum
from typing import List

from pydantic import BaseModel, ConfigDict, Field, field_validator

KEBAB_CASE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
SEMVER = re.compile(r"^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$")


class SkillStatus(str, Enum):
    DRAFT = "draft"
    EXPERIMENTAL = "experimental"
    STABLE = "stable"
    DEPRECATED = "deprecated"


class ParamType(str, Enum):
    STRING = "string"
    INTEGER = "integer"
    NUMBER = "number"
    BOOLEAN = "boolean"
    ARRAY = "array"
    OBJECT = "object"


class SkillParam(BaseModel):
    """One declared input or output of a skill."""

    model_config = ConfigDict(extra="forbid")

    name: str = Field(min_length=1)
    type: ParamType
    description: str = Field(min_length=1)
    required: bool = True


class SkillFrontmatter(BaseModel):
    """Validated YAML frontmatter of a SKILL.md file."""

    model_config = ConfigDict(extra="forbid")

    name: str
    description: str = Field(min_length=1)
    version: str
    status: SkillStatus
    owner: str = Field(min_length=1)
    tags: List[str] = Field(min_length=1)
    when_to_use: List[str] = Field(min_length=1)
    when_not_to_use: List[str] = Field(min_length=1)
    inputs: List[SkillParam] = Field(min_length=1)
    outputs: List[SkillParam] = Field(min_length=1)
    side_effects: List[str]
    idempotent: bool
    timeout_seconds: int = Field(gt=0, le=3600)
    requires_approval: bool
    required_tools: List[str]
    permissions: List[str]

    @field_validator("name")
    @classmethod
    def _name_is_kebab_case(cls, value: str) -> str:
        if not KEBAB_CASE.match(value):
            raise ValueError(f"name must be kebab-case (lowercase letters, digits, hyphens): {value!r}")
        return value

    @field_validator("version")
    @classmethod
    def _version_is_semver(cls, value: str) -> str:
        if not SEMVER.match(value):
            raise ValueError(f"version must be semantic (e.g. 1.0.0): {value!r}")
        return value

    @field_validator("tags")
    @classmethod
    def _tags_are_kebab_case(cls, value: List[str]) -> List[str]:
        for tag in value:
            if not KEBAB_CASE.match(tag):
                raise ValueError(f"tag must be kebab-case: {tag!r}")
        return value
