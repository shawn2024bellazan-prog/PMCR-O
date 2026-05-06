---
title: "Module 09 — The Commercial Model"
---

# Module 09 — The Commercial Model

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: P11, P7, P8, P4*

---

## What You Actually Own

When you run a PMCRO cycle, you produce three things:

1. **The artifact** — a file, a report, a form submission, a browser action. The thing the Maker built.
2. **The Trail** — the typed, cross-phase record of everything that happened.
3. **The constraint** — the behavioral law the Reflector crystallized from what went right and wrong.

Most people building AI systems think they own the artifact. They do not understand that the Trail and the constraint are worth more.

An artifact can be produced by anyone. A Trail with fifty earned constraints on a specific domain — county property records, Indeed application flows, Facebook post cadences — represents fifty cycles of real-world experience. Someone who buys that Trail skips fifty failures. That is what you are selling.

---

## Your Three Revenue Verticals (Already Built or Buildable Now)

You have something most people trying to enter the AI agent market do not have: a running substrate with real MCP actuators and a Playwright server that already works. Here is exactly how each of your existing capabilities becomes revenue.

---

### Vertical 1 — Vacant Property Intelligence

**What you already have:** 300+ properties you can research. A Playwright MCP server. PowerShell scripts that already automate the lookup flow.

**What you build next:** A `PropertyResearchAgent` skill that runs as a PMCRO cycle. The agent navigates to county assessor sites, pulls ownership data, tax status, last sale date, and vacancy indicators. It produces a structured `PropertyResearchFrame` per property. After 300 cycles, you have a constraint set that knows exactly how every county site in your area behaves — which ones rate-limit, which ones need a delay between requests, which ones have different selectors for owner-occupied vs. investor-owned.

**Revenue paths:**

*Path A — Direct.* You already know which houses are vacant and which owners haven't paid taxes. That is motivated seller data. Real estate investors pay for this. The going rate for a motivated seller list with property details is $0.50–$5.00 per lead depending on data quality. 300 properties is $150–$1,500 per list. You can refresh this weekly and sell freshly updated lists.

*Path B — Trail Licensing.* After 300 cycles, package your `PropertyResearchAgent` skill with its earned constraints and sell it. A wholesaler in another city who wants to build the same capability pays you for the Trail. They get the constraints you earned. You get ongoing royalties as their cycles improve the shared Trail.

*Path C — Done-for-you service.* You run the research for other investors. They give you a county and a criteria set. Your agent runs the cycles. You deliver a spreadsheet. This is a service business powered by automation — your labor cost is near zero because the agent does the work.

---

### Vertical 2 — Automated Job Applications (Indeed + Others)

**What you already have:** A Playwright MCP server. The idea already validated by your manual testing.

**What you build next:** An `IndeedApplicationAgent` skill. The agent searches Indeed for jobs matching a criteria set, navigates to each listing, fills the application form, and submits. It produces an `ApplicationFrame` per job: job title, company, application status, any questions that required human answers, and any CAPTCHA events.

The key insight here is P7. Every CAPTCHA the agent encounters, every form field it does not understand, every site that blocks automated submissions — each is a fracture that earns a constraint. By cycle 50, your agent knows the behavioral fingerprint of Indeed's bot detection and navigates around it.

**Revenue paths:**

*Path A — Personal use.* You apply to 50 jobs a day instead of 5. Your application volume goes up 10x. This is not revenue — it is leverage.

*Path B — Productize for job seekers.* Charge $49/month for automated job application on their behalf. The agent applies to 20 jobs per day using their resume. You run the cycles. They get the interviews. Your marginal cost per customer is near zero — the agent does the work.

*Path C — White-label for staffing agencies.* Staffing agencies need to fill candidate pipelines. Your agent auto-applies to jobs and surfaces matches. Sell the service B2B. Staffing agencies have budget. $500–$2,000/month per agency for automated candidate-job matching and application.

*Path D — Trail licensing.* Your IndeedApplicationAgent after 500 cycles has deep constraints on Indeed's application flow, form field patterns, CAPTCHA timing, and success rates by job category. Sell the Trail to anyone building job application automation.

---

### Vertical 3 — Social Media Presence Automation

**What you already have:** The idea. The Playwright server. The ability to navigate and post.

**What you build next:** A `SocialPresenceAgent` skill with a daily cadence. The Planner reads a content calendar (a simple SKILL.md or JSON file you maintain). The Maker drafts posts matched to your brand voice. The DispatchExecutor fires the Playwright actuator to navigate to Facebook (or LinkedIn, or wherever) and post.

The "ghost pages" idea you mentioned is real and it works. You do not need to be present. The agent posts on your behalf. You set the tone once in the skill instructions. It runs.

**The HIL flywheel for social media:** You start with manual approval — every post the Maker drafts, you review before the DispatchExecutor fires. This is the human-in-the-loop gate. Over time, as the posts get better and your corrections become fewer, you reduce the gate to weekly review, then monthly, then never. The escalation rate drops because the agent learned your voice.

**Revenue paths:**

*Path A — Personal brand.* Your PMCRO journey is itself content. An agent that posts daily about building an AI agent company — the constraints earned, the fractures fixed, the cycles run — is a personal brand builder. Audience first, monetize later.

*Path B — Social media management service.* Run the same setup for small businesses. Charge $200–$500/month for daily posting on Facebook, LinkedIn, and Instagram. Your cost is the Aspire host running cycles. Their benefit is consistent presence without hiring a social media manager.

*Path C — Content agency at scale.* Add more "ghost pages" (client accounts). Each client is a separate `master_context` injected into the IntentEnvelope. The agent knows who it is posting for because the identity is injected. 10 clients at $300/month is $3,000 MRR with one operator (you) reviewing outputs weekly.

---

## The Compounding Structure

Here is the thing that makes this different from freelancing or a one-time product: these three verticals compound into each other.

The constraints your PropertyResearchAgent earns on county assessor sites also apply to any other web research task. The CAPTCHA-handling constraints your IndeedApplicationAgent earns apply to the SocialPresenceAgent navigating Facebook. The tone constraints your SocialPresenceAgent earns about your voice apply to any content the Maker produces.

Every cycle you run for personal use produces constraints that make your commercial services better. Every cycle you run for a client produces constraints that make your personal tools better. The system is one compound engine.

---

## The HIL Gradient: From Manual to Autonomous

You described this exactly right. Right now you are doing everything manually. That is where everyone starts. The path forward is not to automate everything at once — it is to automate one gate at a time.

**Stage 1 (now):** Every action requires your direct input. You are the Planner, Maker, and Dispatcher. The agent is your tool.

**Stage 2 (next 30 cycles):** The Planner runs autonomously. You review the plan before the Maker builds. You are the gate between planning and production.

**Stage 3 (after 100 cycles):** The Planner and Maker run autonomously. You review the dispatch decisions before the DispatchExecutor fires. You are the gate between production and world impact.

**Stage 4 (after 300 cycles):** Everything runs autonomously. You review outputs weekly. The ESCALATE rate is below 5%. The system has earned enough constraints to handle almost everything without you.

**Stage 5 (the company):** You are reviewing Trails, not outputs. You are setting direction, not approving individual actions. The agent company runs. You build the next one.

The walk past a tree and see the flyer idea — that intuition is correct. The agent company's output should eventually be visible in the physical world: leads followed up, jobs applied for, posts published, properties researched. The Trail is the backend. The world is the frontend.

---

> **Next:** [Module 10 — The Agent Company Blueprint](10-agent-company-blueprint.md)
