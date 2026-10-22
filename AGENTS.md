# Fork contributor guidance

This is the EyeAuras fork on `feature/eyeauras-edition`. [The library project](ClickableTransparentOverlay/ClickableTransparentOverlay.csproj) defines the fork package identity, framework, dependencies, and AOT settings. Upstream README/package links are reference material, not evidence of this fork's release artifacts. Preserve [LICENSE](LICENSE) and upstream attribution.

`ClickableTransparentOverlay/` owns the Win32/Direct3D11 overlay, ImGui renderer, and input handling; [Examples/](Examples) contains local consumers. A `net8.0` target does not make this Win32/D3D implementation cross-platform. Preserve render-thread/device/resource lifetime, frame allocation constraints, and mouse-capture-based click-through behavior; keyboard capture alone must not disable click-through.

For implementation changes, build the affected project/examples and name what needs a real Windows overlay run. Compilation does not establish rendering, focus, mouse input, or shutdown behavior. Documentation-only work needs factual, local-link, and diff checks with unrun runtime checks disclosed.

Inspect [.github/workflows/publish.yml](.github/workflows/publish.yml) before release work: branch builds and tag creation have different effects, including NuGet/GitHub publication for tags. Documentation maintenance does not authorize a release tag, publishing, or launching example windows.

## Independent architecture and review

Start each delegated architect, advisor, and reviewer with empty conversation context (`fork_turns="none"` with `spawn_agent`, or the host's equivalent). Provide only the concrete question, repository path and relevant revisions, scope, constraints, and raw evidence locations. Keep the brief focused on the actual topic; omit the author's reasoning, preferred verdict, and earlier agents' conclusions.

Each agent independently inspects the relevant code, contracts, and examples and reports its own evidence, conclusions, and uncertainty. The final reviewer must be separate from the implementer and design advisor, remain read-only, and inspect the final diff at the named revision. Disclose unavailable independent review rather than presenting self-review as independent.
