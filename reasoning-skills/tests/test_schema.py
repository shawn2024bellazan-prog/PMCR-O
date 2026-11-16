"""Tests for SKILL.md frontmatter validation."""

from __future__ import annotations

import copy

import pytest
from pydantic import ValidationError

from reasoning_skills.schema import SkillFrontmatter, SkillStatus

VALID = {
    "name": "example-skill",
    "description": "An example skill.",
    "version": "1.0.0",
    "status": "stable",
    "owner": "tooensure",
    "tags": ["reasoning", "example"],
    "when_to_use": ["When testing."],
    "when_not_to_use": ["When not testing."],
    "inputs": [{"name": "task", "type": "string", "description": "The task."}],
    "outputs": [{"name": "answer", "type": "string", "description": "The answer."}],
    "side_effects": [],
    "idempotent": True,
    "timeout_seconds": 60,
    "requires_approval": False,
    "required_tools": [],
    "permissions": [],
}


def make(**overrides):
    data = copy.deepcopy(VALID)
    data.update(overrides)
    return data


def test_valid_frontmatter_parses():
    fm = SkillFrontmatter.model_validate(VALID)
    assert fm.name == "example-skill"
    assert fm.status is SkillStatus.STABLE
    assert fm.inputs[0].required is True


def test_bad_status_enum_raises():
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(status="finished"))


def test_bad_param_type_raises():
    bad = make(inputs=[{"name": "task", "type": "text", "description": "x"}])
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(bad)


@pytest.mark.parametrize("field", ["name", "description", "version", "status", "owner", "tags", "inputs", "idempotent", "timeout_seconds"])
def test_missing_required_field_raises(field):
    data = make()
    del data[field]
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(data)


@pytest.mark.parametrize("name", ["Chain_Of_Thought", "chain of thought", "-leading", "trailing-", "double--hyphen", "UPPER", ""])
def test_non_kebab_case_name_raises(name):
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(name=name))


@pytest.mark.parametrize("name", ["react", "chain-of-thought", "a1-b2"])
def test_kebab_case_name_accepted(name):
    assert SkillFrontmatter.model_validate(make(name=name)).name == name


def test_bad_version_raises():
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(version="v1"))


def test_bad_tag_raises():
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(tags=["Not Kebab"]))


def test_unknown_field_raises():
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(surprise="x"))


@pytest.mark.parametrize("timeout", [0, -5, 3601])
def test_timeout_out_of_range_raises(timeout):
    with pytest.raises(ValidationError):
        SkillFrontmatter.model_validate(make(timeout_seconds=timeout))
