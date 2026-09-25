# GitHub Publishing Checklist

Start with a private repository named `keinishkata-kushta`. Publishing source
code does not deploy the website or make localhost accessible to visitors.

## Before the First Push

- Confirm the GitHub owner, repository name, and private visibility.
- Review the Git author name and email. Use the GitHub-provided noreply email
  from your account settings if you do not want a personal address in commits.
- Keep appsettings.json, appsettings.Development.json, User Secrets, uploads,
  databases, backups, IDE state, and generated build output out of the commit.
- Review the staged diff for credentials and private information. Ignore rules
  are a safety net, not a secret scanner. Never use `git add -f` for private files.
- Verify that the example configuration and README work on a fresh checkout.

The existing local settings and files stay on disk; ignoring them does not delete
them. The repository should be rooted beside `KeinishkataKushta.sln`, not in the
parent folder containing archives and design references.

## Before Making It Public

- Review the complete Git history, not just the latest files. If a credential
  ever reaches GitHub, revoke/rotate it; deleting the file alone is not sufficient.
- Confirm permission to publicly redistribute all photographs, branding, and
  design assets. Keep all third-party license and attribution requirements.
- Review business contact information in `Infrastructure/SiteContact.cs` and
  any contact information embedded in images before publishing.
- Add screenshots using fictional/sample content, with no guests, enquiries,
  reservation notes, credentials, or private browser details visible.
- Select a code license deliberately. Do not apply it to third-party or branding
  assets without the necessary rights.
- Re-run tests and review the README's claims and limitations.

## Everyday Workflow

Make focused commits for completed changes instead of creating separate
repositories for each feature. Check `git status` and the staged diff before each
commit. Push only to the intended repository. Do not rewrite shared history or
force-push as part of the normal workflow.
