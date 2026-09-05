# Contributing Guidelines

These rules govern all work in this repository.

## Required Workflow

1. **Start with a GitHub issue**
   - Every change must be tracked by a GitHub issue before work begins.
   - The issue should describe the problem, goal, or bug being addressed.
   - Work that does not have an issue should not be started or merged.

2. **Use a dedicated branch**
   - Do not work directly on `main`.
   - Create a branch for each issue or unit of work.
   - Branch names should clearly describe the work being done.

3. **Open a pull request**
   - All code, documentation, configuration, and workflow changes must be submitted through a pull request.
   - Pull requests should reference the related GitHub issue.
   - Pull requests should include a clear summary of what changed and why.

4. **Merge into `main` only through PR review**
   - `main` is the protected integration branch.
   - Changes should be reviewed before merge.
   - Do not bypass the pull request process.

## Repository Hygiene Expectations

- Keep changes focused and scoped to the issue being addressed.
- Avoid mixing unrelated fixes in a single branch or pull request.
- Update documentation when behavior, setup, or workflows change.
- Run relevant validation before requesting review.
- Do not commit secrets, credentials, or local-only configuration.
- Remove temporary files, debug artifacts, and test output before merge.
- Prefer small, reviewable pull requests over large unstructured changes.

## Pull Request Checklist

Before merging, confirm that:

- [ ] A GitHub issue exists for the work
- [ ] The change was made on a non-`main` branch
- [ ] A pull request was opened for review
- [ ] The pull request references the related issue
- [ ] Relevant tests, builds, or checks were run
- [ ] Documentation was updated when needed
- [ ] No secrets or unnecessary generated files were committed

## Maintainer Guidance

Maintainers should reject or request changes for work that:

- does not have a GitHub issue
- was done directly on `main`
- bypasses pull request review
- contains unrelated changes
- lowers repository hygiene or leaves incomplete follow-up work
