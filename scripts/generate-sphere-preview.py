import math
from pathlib import Path

count = 2800
golden_angle = math.pi * (3 - math.sqrt(5))
points = []
for i in range(count):
    y = 1 - i / (count - 1) * 2
    radius = math.sqrt(1 - y * y)
    angle = golden_angle * i
    x, z = math.cos(angle) * radius, math.sin(angle) * radius
    depth = z * .5 + .5
    perspective = 1 / (1.35 - z * .28)
    color = tuple(round((a + (b - a) * depth) * 255) for a, b in zip((.02, .36, .95), (.24, .66, 1)))
    points.append(f'<circle cx="{x * perspective * 1.84 * 500:.2f}" cy="{-y * perspective * 1.84 * 500:.2f}" r="{(2.6 + depth * 1.8) * perspective / 2:.2f}" fill="#{color[0]:02x}{color[1]:02x}{color[2]:02x}" opacity="{(.32 + depth * .68) * .72:.3f}"/>')

target = Path(__file__).resolve().parents[1] / 'Neurix/wwwroot/images/neurix-sphere-preview.svg'
target.write_text('<svg xmlns="http://www.w3.org/2000/svg" viewBox="-700 -700 1400 1400">' + ''.join(points) + '</svg>', encoding='utf-8')
