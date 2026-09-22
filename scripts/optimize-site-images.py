from pathlib import Path
from PIL import Image, ImageOps

root = Path(__file__).resolve().parents[1] / 'Neurix/wwwroot/images'
before = after = count = 0
for source in sorted(root.iterdir()):
    if source.suffix.lower() not in ('.png', '.jpg', '.jpeg') or source.stat().st_size < 150_000:
        continue
    target = source.with_suffix('.webp')
    with Image.open(source) as original:
        if getattr(original, 'is_animated', False):
            continue
        optimized = ImageOps.exif_transpose(original)
        optimized.thumbnail((1920, 1920), Image.Resampling.LANCZOS)
        optimized.save(target, 'WEBP', quality=85, method=6)
    with Image.open(target) as verification:
        verification.load()
        assert verification.width > 0 and verification.height > 0
    before += source.stat().st_size
    after += target.stat().st_size
    count += 1
print(f'{count} images: {before:,} -> {after:,} bytes ({(1-after/before)*100:.1f}% smaller)')
