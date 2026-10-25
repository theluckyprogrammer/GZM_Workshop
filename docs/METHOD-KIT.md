# Method kit: spec-first, capability-by-capability development with coding agents

A portable description of the working method proven on JobMatch (a modular monolith built
mostly by Claude Code and Kilo agents). Hand this file to a coding agent in a new repository
and ask it to bootstrap the method (Part 13). Everything project-specific is marked
`<PLACEHOLDER>`; everything else is the method itself and should be copied as is.

Audience: a coding agent (Claude Code or Kilo) and the person who owns the project, called
**the producer** below.

---

## Part 1: The idea in five sentences

1. Starting from invented interfaces produces the wrong interfaces; starting from code produces
   a working mess. So behaviour is described first, step by step, and structure is *derived*
   from that description.
2. The unit of structure is the **module**; the unit of work is the **capability**: one named
   action with input, output and numbered rules, owned by exactly one module.
3. Each capability lives in one markdown file and moves through seven steps, from plain
   sentences to tested code. Nothing from a later step appears early.
4. Three registers are kept in the same session as the work: a worklog of checkpoints, a
   findings register of things discovered on the way, and a glossary of agreed terms.
5. Larger work is fanned out to parallel agents, one brief and one worktree each. A single
   coordinator merges, validates on fresh builds and deploys.

---

## Part 2: Module and capability

**Module.** One project/package (e.g. one csproj). Owns its contract and its storage schema.
Modules do not know each other's internals. `<LIST THE MODULES>`.

**Capability.** A single action the system performs: "normalize compensation from raw text",
"reserve entries in the sending ledger before dispatch". It has input, output and rules. It
belongs to one module. A capability is a folder inside the module in code and one file in
`spec/capabilities/` in the spec.

Behaviour is described for capabilities, never for modules: a module has no branching and no
repetition, a capability does.

Slug format: `<module-prefix>-<NNN>-<verb-phrase>`, e.g. `ident-005-resolve-publication-identity`.
The prefix is 3–6 letters per module; numbers are sequential within a module and never reused.

---

## Part 3: The seven steps

For one capability:

1. **Describe what happens.** No classes, no technology. Who triggers it, what comes in, what
   comes out, the rules. Every rule is numbered; the number later becomes part of a test name.
   Sentences only: no `if`, no loop syntax, no labels, no early-return notation. "Go through the
   candidates until one matches the blocking key" is fine; `for (…) { if (…) break; }` is not.
   Coarse steps first, then refine the ones that hide complexity, in the same numbered list.
2. **Write flat procedural pseudocode.** Loops, conditionals and switches stay visible. Nothing
   is beautified before the complexity is understood. (Working material only; it is not stored
   in the capability file.)
3. **Flatten the nesting.** Extract nested loops and conditionals; guard clauses instead of
   else. Every fragment should read without scrolling.
4. **Find repetition.** Two similar places is an observation, three is a pattern. Similar code
   is not the same as code that changes for the same reason. Interface and class names may
   appear from this step on.
5. **Build the hierarchy.** Interfaces and composition by default. A base class only when "is a
   kind of" genuinely holds. Logging, retry and measurement are decorators, never inheritance.
   Infrastructure vocabulary (file, HTTP, SQL, queue, AI SDK) may appear from this step on.
6. **Decide the contract.** What leaves the module and what stays inside. This is a decision,
   not a formality (Part 5). Code in `<LANGUAGE>` may appear from this step on.
7. **Write tests, then code, outside-in.** Red acceptance test first, then downwards (Part 6).

**Full path versus light path.** The full seven steps are reserved for capabilities whose rules
are non-obvious and whose mistakes are expensive: `<LIST HIGH-RISK CAPABILITIES, e.g.
deduplication, money/units normalization, ranking funnel, AI budget, sending ledger, model
output validation>`. For CRUD, configuration and read endpoints, "What happens" plus "Rules"
plus tests is enough. Roughly one third of capabilities take the full path.

**Skipping the method.** Trivia, questions about existing code and one-line fixes skip it. The
agent says in one line which path it took. Bug fixes still go tests-first, and the spec is
updated when a fix changes a rule.

---

## Part 4: The capability file

`spec/capabilities/<slug>.md`:

```markdown
---
capability: <slug>
module: <Module>
state: draft          # draft | published
---

## What happens
## Rules              numbered; the numbers appear in test names
## Types and methods  filled in at step 4
## Error behaviour    only for capabilities that talk to external sources
## What we learned    what building it revealed that the description missed
```

- "What happens" carries all of step 1. There is no pseudocode section: if the description
  needs pseudocode to be understood, it was written at the wrong level.
- Levels of description are sections in one file, not separate files.
- **"What we learned" is the section everyone skips and the one with the most value.** It
  records where the description was wrong and where a contract leaked.
- Later changes to an implemented capability are recorded as a dated **amendment**: a header
  line "Amended YYYY-MM-DD: …" at the top, new rules appended with new numbers (existing rule
  numbers are never renumbered, because tests and code reference them), changed rules edited
  in place, and a line in "What we learned".

Keep a `spec/capabilities/_template.md` that shows the target shape and the right level of
detail for step 1, using one worked example from the project.

---

## Part 5: Freezing, draft and published

**draft**: nothing outside the module depends on it. Rewrite the contract freely, however much
code inside the module already uses it. Changes are not logged.

**published**: the type is in the module's contracts package and another module uses it. An
upward change now requires an explicit decision. The transition to published is a named
decision with a date.

- **Publish as late as possible.** A type enters `*.Contracts` when a second module actually
  needs it, never because it might.
- During the first end-to-end slice, everything is draft, contracts included.
- **Pre-existing design documents are hypotheses.** Any code in documents written before the
  method (module specs, architecture docs) is invented, not derived, and never counts as
  published. Read them for domain vocabulary, enumerated cases and rules; never for type
  names, interface shapes or field sets. When a capability description conflicts with them,
  the description wins and the document is corrected afterwards.

When a published contract is challenged from below, classify the reason:

1. Domain misunderstood: valid, write an ADR.
2. Contract leaked implementation: valid, fix the abstraction (not "add a parameter").
3. A caller finds it more convenient: refuse. This is how contracts rot.

Record each leak in "What we learned" with one sentence of cause. Their frequency is the
cheapest signal of contract quality.

---

## Part 6: Tests

Each spec section owns a level of test:

- "What happens" and "Rules" → the capability acceptance test, plus example-based tests for
  rules that describe an algorithm.
- "Types and methods" → port contract tests and unit tests.
- "Error behaviour" → one contract test case per listed situation.

Rules:

- Red acceptance test first, then work downwards. Never the other way round.
- Test names carry the rule number: `<Capability>Rule<N>_<Behaviour>`, e.g.
  `Portal017Rule4_AppliedCheckbox_SavesOnCheckAndUncheck`.
- Test doubles only at ports (module boundaries). Real objects inside a module. Hand-written
  doubles, no mocking framework: steps 3–5 are continuous refactoring, and tests glued to
  internal structure make that refactoring impossible.
- One contract test suite per port, run against every implementation. Build it the moment a
  second implementation exists, not before.
- Never skip, disable or quarantine a test to get green. A test is weakened only when the rule
  it checks was deliberately changed.

---

## Part 7: SOLID at the stage where it works

- **SRP, steps 2–3.** Ask who would request a change to this fragment. Different requesters
  mean different methods.
- **ISP, steps 3–4.** An implementation that would throw `NotSupportedException` means the
  interface is too wide.
- **DIP, step 5.** Decides what enters the contracts package. Domain code never sees HTTP, SQL,
  queues, files or AI SDKs.
- **OCP, review after step 4.** Adding one more source/provider/variant must not change
  existing code. A switch over a closed, stable enum is fine.
- **LSP, via contract tests**, not by reading.

---

## Part 8: Architecture rules

The project keeps a short numbered list of non-negotiable architecture rules in `AGENTS.md`.
Generic rules that came with this method:

1. Domain code depends on ports only. Concrete types are wired in one composition root.
2. Each module owns its storage schema. No joins across schemas.
3. No single shared contracts package: a small shared kernel (identifiers, result type,
   clock; each addition justified in one sentence) plus one contracts package per module.
4. Every message consumer is idempotent, and its idempotency key is stated.
5. Persistence adapters stay concrete and task-focused. No generic `IRepository` or repository
   base class. Add an abstraction only at a real module boundary; for a test seam, wrap the
   specific method in a narrow interface.
6. Cross-cutting behaviour (logging, retry, measurement) is a decorator.

`<ADD THE PROJECT-SPECIFIC RULES: e.g. "store raw bytes before parsing", "the AI model is the
last stage of the funnel only", "the system degrades without the model", "reserve in the
ledger before sending, confirm after">`.

**When a request breaks a rule**, the agent names the rule, says what breaking it costs and
offers the ADR route. It neither complies silently nor refuses flatly. The agent pushes back
once, clearly, then does what was decided.

---

## Part 9: The conversation protocol for a capability

Applies from the moment a capability slug is named.

**Starting.** Before the first response, read the system overview, the domain model and the
owning module's document. Create `spec/capabilities/<slug>.md` with frontmatter and empty
sections, `state: draft`. Then produce step 1 and nothing else.

**One step per response.** Exactly one step. No preview of the next, no recap of the approved.

**Approval is literally `OK: <n>`.** Nothing else approves: not "sounds good", not "yes", not
silence. Anything else is a comment: revise the same step and present it again. On `OK: <n>`,
write the approved content into the capability file, then produce step n+1 in the same
response.

**Going back.** `BACK: <n>` reopens step n. Everything derived below it is discarded and
re-derived, not patched.

**Questions.** At most three, numbered, at the end, each answerable in one line. If a question
blocks the whole step, ask it instead of producing the step.

**Contradictions.** If the module document or domain model contradicts what is being
described, stop and say so. Never reconcile silently.

**What must not appear early.** No code before step 6. No interface or class name before
step 4. No `if`, loop syntax or labels in step 1. No file, HTTP, SQL, queue or AI SDK
vocabulary before step 5.

**Autopilot mode.** When the producer explicitly asks to go through all steps (or has
durably delegated approvals), that request is the approval for the current capability. The
agent records a checkpoint in `WORKLOG.md` after each meaningful step, runs focused validation
after edits, and continues until the capability is complete or genuinely blocked. It stops
only for an unresolved contradiction, missing design information, or an execution failure it
cannot repair locally. It finishes with a summary of completed steps, remaining gaps and
questions.

**Communication defaults** (adjust to the producer): `<DISCUSSION LANGUAGE>` for discussion,
English for code, identifiers and spec files. Lead with the answer. Short over complete: give
the summary, offer the detail. Facts as facts, recommendations as "should". Say when guessing.
No tables.

---

## Part 10: The three registers (plus ADRs)

All are append-at-the-top, written in the same session as the work, and never rewritten
historically.

### WORKLOG.md: development checkpoints

A new entry at the top after a meaningful step approval, implementation milestone, test
milestone, merge, deploy or commit. Shape:

```markdown
## YYYY-MM-DD — <capability or run>: <one-line outcome>

- Capability / module: <slug(s)> / <module(s)>
- Method step: <n, or "debug", "triage", "bug fix (method skipped)">
- Rules: <rule numbers touched>
- State: <what was done; branch; committed or UNCOMMITTED, said explicitly>
- Validation: <exact commands and real counts, e.g. "build 0W/0E; tests 1041 pass / 0 fail / 1 skip">
- Outcome: <what is now true>
- Excluded scope: <what was deliberately not done>
- Next action: <one concrete next step>
```

The worklog is development history, not runtime state; never confuse the two.

### Docs/findings.md: things found along the way

Something missing, documentation that disagrees with code, a misleading name or setting, a
feature only half wired. Record every such finding in the same session it was found, even when
fixed on the spot.

```markdown
### F-NNN · <one-line title>

- **Date:** YYYY-MM-DD · **Commit:** `<git rev-parse --short HEAD>`
- **Description:** one to three sentences naming the file, setting or behaviour.
- **Status:** open | fixed in `<commit>` | rejected: <reason>
```

Newest on top, next free number. Never delete an entry; update its status. QA defects awaiting
assignment live separately in `Docs/follow-ups.md`. Pull before numbering: parallel branches
collide on F-numbers, and the coordinator renumbers at merge.

### spec/glossary.md: shared language

Every newly agreed domain term as `### English term (<other-language term>)` followed by a short
definition. Specifications use the canonical English term. Terms are added only after
agreement.

### adr/: decision records

`adr/ADR-NNN-<slug>.md` with context, decision, alternatives considered and consequences.
Written when a published contract changes for a domain reason, when an architecture rule is
deliberately broken, or when a technology/vendor choice is made.

---

## Part 11: Roles

The method maps onto agent roles. In Kilo they are modes (`.kilo/instructions/roles.md`,
custom modes in `.kilo/agents/`); in Claude Code they are subagent prompts or simply the stance
the session takes. When an operation spans roles, finish the current role's part, say which
role takes over and why, and stop.

- **Ask**: questions about code, spec, data or past decisions. Changes nothing; names findings
  and hands over.
- **Plan**: steps 1–3. Creates the capability file, writes step 1, writes task briefs and
  glossary terms. No code, no tests. Stops on contradictions. Hands over after `OK: 3`.
- **Architect**: steps 4–6, ADRs, changes to published contracts, conflicts with architecture
  rules. Reviews OCP and ISP. Hands over after `OK: 6`.
- **Test Engineer**: tests of step 7, outside-in, rule numbers in names, doubles only at ports.
  Hands over a list of red tests.
- **Code**: makes red tests green with the smallest change; also trivia, migrations, scripts,
  worklog and findings entries. Reports real test counts, failures included.
- **Debug**: reproduce, isolate, fix the root cause ("flaky" is not a root cause), record a
  finding. A fix that changes a rule goes back to Plan/Architect.
- **Code Skeptic**: reviews a diff or branch, edits nothing but markdown. Order: correctness
  (concrete input → wrong output), architecture rules, method (step order, rule numbers in test
  names, nothing early), idempotency keys, doubles only at ports, missing worklog/findings.
  Each finding: file:line, what fails, realistic scenario, severity. No style nits.
- **Code Simplifier**: only after tests are green; behaviour identical; never touches published
  contracts; removes duplication only at three occurrences.

---

## Part 12: Parallel agent runs

Used when a piece of work splits into independent tasks (e.g. spec+docs, tests, code per
module, verification).

### Run layout

```
agent-tasks/
  README.md                  working rules for task agents (below)
  _template.md               task brief template
  runs/<run-id>/
    tasks/<task>.md          one self-contained brief per agent
    <run-id>-run-summary.md  coordinator's log: task states, decisions D-NN, validation
```

### Task brief template

```markdown
---
task: <run-id>/<task>
depends_on: []
capabilities: [<slugs>]
allowed_paths: []
forbidden_paths: []
---

# Task: <name> — <one observable outcome>

## Context (what is already done on the base)
## Task (numbered; hypotheses to verify, not conclusions)
## Environment (exact commands; which databases/services are test-only; what is forbidden)
## Scope (allowed paths; out of scope, e.g. "spec/, WORKLOG.md: coordinator does it")
## Validation and artifacts (exact commands with expected results; commits on own branch)
## Final report (per item: cause, file, kind of fix; worktree, branch, files, commands, results, limitations)
```

A brief must survive isolation: the agent starts with no memory. Name the files with paths,
list hypotheses rather than conclusions, and tell the agent to re-measure the baseline itself:
counts in a brief may come from stale binaries.

### Working rules for task agents

1. One brief, one worktree, one owner. Never work in the main checkout.
2. All tasks of a run branch from one base SHA, resolved once. Branch name
   `task/<run-id>/<task>`. A dependent task (e.g. code after tests) branches from its
   dependency's branch.
3. At most three agents at the same time, split along disjoint files.
4. Read `AGENTS.md` and module-level instructions before editing. Follow the method.
5. Commit on the task's own branch. Never merge into the base branch. Never push.
6. Forward-only migrations. Never write to production data.
7. Shared services (local databases, ports) are shared between parallel agents: a failure from
   a collision deserves a retry before a verdict.
8. Git-ignored private data is absent from worktrees; never copy it. Point read-only runs at the
   main checkout's directory.
9. Run focused tests after the first edit and the full suite before handoff.

### Starting agents

- **Kilo**: the `agent_manager` tool, `mode: "worktree"`, one entry per task (`name`,
  `branchName`, self-contained `prompt`). **Omit the `worktreeID` key entirely**: an explicit
  `null` is rejected ("worktreeID requires mode local"). Pass `model`/`provider` only when the
  producer asked, resolved via `agent_manager_models`. Every agent is a visible session so the
  producer can watch, prompt and answer questions. Monitor with `action: "list"`, answer with
  `action: "answer"` using the exact option labels. A headless script fallback
  (`git worktree add` + `kilo run --auto`) is only for environments without the UI.
- **Claude Code**: the Agent tool with `isolation: "worktree"`, one call per task in a single
  message so they run concurrently; the prompt is the brief.

### Coordinator duties

1. Bookkeeping before agents start: verify open findings the run depends on and record the
   result as a decision.
2. Merge finished branches one at a time (`--no-ff`). After **every** merge run an explicit
   build. Resolve conflicts in shared files (`WORKLOG.md`, `Docs/findings.md`) centrally:
   keep both entries, renumber F-numbers if they collide.
3. Run a Code Skeptic pass on the merged result; fix blockers before continuing; send the rest
   to findings. A final Code Simplifier pass when the run is green.
4. Combined validation on fresh builds, then deploy if part of the run.
5. Keep the run summary current. Every non-trivial choice is a decision entry:

```markdown
- **D-NN**: <decision in one sentence>
  - **Context:** <what forced it>
  - **Alternatives:** <what else was possible>
  - **Why this one:** <reason>
  - **Effect:** <consequence; how easy to reverse>
```

---

## Part 13: Validation discipline (lessons that cost real time)

- **Only fresh builds count.** Build with `--no-incremental` (or the stack's equivalent) before
  claiming green. "Green" tests on stale binaries hid a non-compiling commit more than once.
- **Build every solution/workspace.** If a project lives outside the main solution, a green
  solution build says nothing about it. Prefer one solution containing everything.
- **Report real numbers**: "1041 pass / 0 fail / 1 skip", with the command. Name pre-existing
  failures explicitly (by finding number) instead of folding them into "mostly green".
- **Triage each red test** with the full log: product regression or stale test expectation.
  Never weaken a test to confirm a deliberate rule change; change it only to match the rule.
- **Root cause, not symptom.** Instrument temporarily, prove the hypothesis, apply the minimal
  fix, revert the instrumentation, record the finding.
- **Deploy is part of done** when the capability is meant to run: deploy, smoke-check, read the
  logs, then write the worklog entry.

---

## Part 14: Order of work and Definition of Done

**Order.** First the walking skeleton: one thin slice through the whole system on real
infrastructure (`<e.g. one source query, one stored snapshot, one parse, one record, one user,
one output>`), with no advanced features. Its purpose is to find out whether contract shapes
survive contact with reality before fifty of them are frozen; the project layout is a result of
building it, not a scaffold laid out in advance. After that, capabilities are ordered **by
risk, not by dependency**: least understood first.

**Definition of Done for a capability:**

- every numbered rule has at least one passing test
- "What we learned" is filled in
- `state` in the frontmatter is accurate
- anything added to the shared kernel carries a sentence of justification
- any new interface with a single implementation exists only as a module boundary or test seam
- telemetry exists for the new path
- a worklog entry records the validation

**Added only when it hurts, never in advance:** an architecture test for module isolation (when
a second module exists), a contract-test base class (when a second implementation of a port
exists), CI (after the first capability has gone through the full method), path-scoped
instruction files (when `AGENTS.md` passes ~150 lines and half of it applies to one directory),
spec drift checking (when published capabilities start drifting unnoticed), a pre-commit hook
(when CI has caught the same class of mistake twice).

---

## Part 15: Bootstrapping the method in a new repository

Instructions for the agent receiving this file.

**1. Ask the producer** (at most these, in one message):
   1. Project name and one-paragraph purpose.
   2. Language/stack and how a module maps to a project (csproj, package, crate…).
   3. The module list, or "derive it with me from the domain".
   4. Which capabilities are high-risk (full seven steps).
   5. Project-specific architecture rules.
   6. Discussion language; Kilo, Claude Code or both.

   Existing design documents in the repo are read as hypotheses (Part 5), not as contracts.

**2. Create these files**, filling placeholders from the answers:

- `AGENTS.md`: the canonical operational instructions, under ~250 lines. Sections: what this
  repository is; the seven steps (Part 3); shared language and checkpoints (Part 10, short
  form); running a capability session (Part 9); capability file (Part 4); freezing (Part 5);
  architecture rules (Part 8); SOLID by stage (Part 7); tests (Part 6); order of work
  (Part 14); communication; starting parallel agents (Part 12, short form). State at the top
  that `spec/00-method.md` wins when they disagree.
- `CLAUDE.md`: a single line `@AGENTS.md`, so Claude Code and Kilo read the same file.
- `spec/00-method.md`: the long form: Parts 1–7 and 14 of this kit, with the project's
  purpose. Keep its step count identical to `AGENTS.md`.
- `spec/capabilities/_template.md`: Part 4 plus one worked example of step 1 from this domain.
- `spec/glossary.md`: header and an empty "Terms" section.
- `spec/modules/`: one document per module when the producer has module knowledge; an
  overview and a domain model document first.
- `WORKLOG.md`: title plus the first entry ("method bootstrapped").
- `Docs/findings.md`: header with the rules from Part 10, no entries yet.
- `Docs/follow-ups.md`: header only.
- `adr/`: created with the first ADR, not before.
- `agent-tasks/README.md` and `agent-tasks/_template.md`: Part 12.
- For Kilo: `.kilo/instructions/roles.md` (Part 11), `.kilo/agents/code-skeptic.md` (edit
  denied except `*.md`) and `.kilo/agents/code-simplifier.md`, `.kilo/command/agent-manager.md`
  (the start checklist and the `worktreeID` gotcha from Part 12).

**3. Do not create** code projects, CI, hooks or test base classes. They come with the walking
skeleton and Part 14.

**4. Report** the created files and the open placeholders, and propose the walking-skeleton
slice as the first capability set.
