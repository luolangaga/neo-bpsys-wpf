from pathlib import Path

file_ranges = [
    ("neo-bpsys-wpf.Core/AppConstants.cs", 1, 40),
    ("neo-bpsys-wpf/App.xaml.cs", 20, 80),
    ("neo-bpsys-wpf/Services/PluginMarketplaceService.cs", 1, 120),
]

for rel, start, end in file_ranges:
    path = Path(rel)
    print(f"=== {rel} ===")
    lines = path.read_text(encoding="utf-8").splitlines()
    for i, line in enumerate(lines, 1):
        if start <= i <= end:
            print(f"{i}: {line}")
