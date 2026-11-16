# reasoning-skills

A library of agent skills where each skill encodes one LLM reasoning technique, so an agent can pick the right reasoning strategy for a task. Skills follow the Agent Skills `SKILL.md` format and ship as an Agent Plugins `plugin.json` package. A small Python loader validates every skill and renders its template.

Each skill teaches one reasoning family and has the same parts: YAML frontmatter (name, version, status, tags, when to use and not use, inputs, outputs, side effects, approval, tools, permissions), then What this skill does, Instructions, a Jinja2 Template with untrusted inputs in XML tags, Security notes, Examples, and Limitations.

## Skills

| Skill | One line |
|---|---|
| `chain-of-thought` | Write out intermediate steps before the final answer. |
| `tree-of-thought` | Explore several reasoning paths, score and prune them, back up from dead ends. |
| `react` | Interleave Thought, Action, and Observation so each step is grounded in tool results. |
| `reflection-refinement` | Draft, critique against explicit criteria, and revise in a bounded loop. |
| `least-to-most` | Decompose into simpler subproblems and solve them in order. |
| `self-consistency` | Sample several reasoning paths and take the majority answer. |
| `plan-and-solve` | Write an explicit plan first, then execute it and record deviations. |
| `structured-output` | Reason in a scratchpad, then emit JSON that matches a schema. |
| `retrieval-augmented-reasoning` | Answer only from retrieved passages and cite every claim. |
| `tool-augmented-reasoning` | Justify each tool call before it and check the result after it. |
| `meta-prompting` | Have the model write and improve a prompt, tested against examples. |
| `socratic-questioning` | Probe a claim with questions that surface assumptions and gaps. |
| `program-aided-reasoning` | Write the reasoning as code and run it in a sandbox (PAL). |
| `chain-of-verification` | Draft, plan fact-check questions, answer them independently, then revise. |
| `multi-agent-debate` | Independent solvers critique each other over rounds and converge. |
| `verifier-best-of-n` | Generate N candidates and choose by verifier-weighted voting. |
| `adaptive-thinking-budget` | Pick reasoning effort by difficulty and escalate only when a check fails. |

The first twelve cover prompting-level families. The last five were added after checking current surveys and platform documentation (see Sources).

## Not included

Some of what modern reasoning models do is training, not prompting: reinforcement learning with verifiable rewards, process-reward training, and test-time adaptation of weights. These are not skills an agent can invoke, so they have no entry here. The skills above are the techniques you can apply at inference time.

## Install

```bash
pip install -e ".[dev]"
pytest
```

Python 3.10 or later. Runtime dependencies are pydantic, pyyaml, and jinja2.

## Usage

```python
from reasoning_skills import SkillLibrary

library = SkillLibrary.from_directory("skills")

print([s.name for s in library.list()])

# Look up one skill. Raises SkillNotFoundError if it does not exist.
cot = library.get("chain-of-thought")
print(cot.frontmatter.when_to_use)

# Filter by tag and/or status. Filters combine with AND.
for skill in library.search(tag="verification", status="experimental"):
    print(skill.name)

# Render the skill's Jinja2 template. Missing variables raise an error.
prompt = cot.render(task="A shop sells pens at 3 for $4. What do 12 pens cost?")
print(prompt)
```

The loader checks that each frontmatter block matches the schema, that the `name` matches its directory, and that no two skills share a name. `render` runs the template in a Jinja2 sandbox with strict undefined variables.

## Security model

Every template wraps untrusted inputs in XML tags and tells the model to treat their content as data, not instructions. This reduces prompt-injection risk but is not a security control on its own. Each skill's Security notes say what to enforce in code: sandboxing, tool allow-lists, approval for side effects, strict output parsing, and caps on cost-multiplying settings. Skills with real-world effects set `requires_approval: true`.

## Layout

```
plugin.json
pyproject.toml
skills/<skill-name>/SKILL.md
src/reasoning_skills/{schema,loader,__init__}.py
tests/{test_schema,test_loader}.py
```

## Sources

- Wei et al., 2022, chain-of-thought prompting.
- Yao et al., 2023, Tree of Thoughts.
- Yao et al., 2022, ReAct.
- Wang et al., 2022, self-consistency.
- Zhou et al., 2022, least-to-most prompting.
- Gao et al., 2022, PAL: Program-aided Language Models.
- Dhuliawala et al., 2023, Chain-of-Verification.
- Du et al., 2023, multiagent debate.
- ThinkPRM (generative process reward models and verifier-weighted best-of-N).
- Surveys of test-time compute (System-1 to System-2 taxonomy; fixed-budget versus adaptive compute).
- Anthropic extended and adaptive thinking documentation (adaptive thinking and effort levels; manual budgets deprecated on the newest models).
- Common agentic design patterns: ReAct, reflection, planning, tool use, multi-agent collaboration, sequential workflow, human-in-the-loop.
