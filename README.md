# devsum2026-demo

Companion repository for the **DevSum 2026** talk *"Locking Down Containers:
Developer-Friendly Security for Kubernetes Workloads"*.

This repo holds only the **production-style CI/CD pipeline**:

- A small ASP.NET Core 10 web app (`dotnet/`)
- A hardened, chiseled, non-root Dockerfile
- A GitHub Actions workflow that builds, vulnerability-scans, and **keyless-signs** the image

The slides and on-stage demo scripts live in the sibling repository:
**[github.com/ronaldharmsen/DevSum2026](https://github.com/ronaldharmsen/DevSum2026)**.

## What the pipeline does

On every push to `main` (and on manual `workflow_dispatch`):

1. **Build** `Dockerfile` → ASP.NET Core 10 chiseled image, non-root.
2. **Push** to `ghcr.io/ronaldharmsen/devsum2026-demo:<sha>` and `:latest`.
3. **Trivy** scans the pushed image; the build fails on any unfixed HIGH/CRITICAL CVE.
4. **Cosign** signs the image **keyless** via Sigstore — no private key on disk, no
   secret in GitHub. The workflow's OIDC token is the identity; Sigstore's Fulcio
   issues a short-lived certificate and the signature is logged to Rekor.

## What verifies the signature

The talk's Kyverno `ClusterPolicy` (in the [DevSum2026 repo](https://github.com/ronaldharmsen/DevSum2026/blob/main/demo-scripts/03b)) pins on:

```yaml
attestors:
- entries:
  - keyless:
      issuer: https://token.actions.githubusercontent.com
      subject: https://github.com/ronaldharmsen/devsum2026-demo/.github/workflows/build-and-sign.yml@refs/heads/main
```

No public key is stored — Kyverno asks Sigstore to vouch that the named
GitHub Actions workflow really did sign that image digest, on a specific
branch.

## Image

`ghcr.io/ronaldharmsen/devsum2026-demo:latest` (public)

## Verifying manually

```sh
cosign verify \
  --certificate-identity 'https://github.com/ronaldharmsen/devsum2026-demo/.github/workflows/build-and-sign.yml@refs/heads/main' \
  --certificate-oidc-issuer 'https://token.actions.githubusercontent.com' \
  ghcr.io/ronaldharmsen/devsum2026-demo:latest
```

## License

MIT
