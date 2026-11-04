from PIL import Image, ImageDraw

SRC = 'Muelsyse/images/icon/cultivate/draw.png'
DST = 'Muelsyse/images/icon/cultivate/'
BASE = (0, 77, 43, 255)
GREEN = (73, 194, 133, 255)
LIGHT = (126, 224, 175, 255)
DARK = (32, 128, 82, 255)

# --- build a blank icon plate from the existing draw icon -------------------
plate = Image.open(SRC).convert('RGBA')
px = plate.load()
for y in range(12, 52):
    for x in range(12, 52):
        if px[x, y][3] > 0:
            px[x, y] = BASE
plate.save(DST + 'plate.png')

SS = 4
S = 64 * SS


def glyph(draw_fn):
    mask = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(mask)
    draw_fn(d, SS)
    mask = mask.resize((64, 64), Image.LANCZOS)
    out = plate.copy()
    out.alpha_composite(mask)
    return out


def attack(d, s):
    # upward sword: tip, blade, crossguard, grip, pommel
    def P(points):
        return [(x * s, y * s) for x, y in points]
    # dark side of the blade
    d.polygon(P([(32, 14), (37, 23), (37, 37), (32, 37)]), fill=DARK)
    # main blade
    d.polygon(P([(32, 14), (27, 23), (27, 37), (32, 37)]), fill=GREEN)
    # highlight along the left edge
    d.line(P([(31, 18), (29, 24), (29, 33)]), fill=LIGHT, width=int(1.2 * s))
    # crossguard with a slight downward curve
    d.polygon(P([(21, 37), (43, 37), (41, 41), (23, 41)]), fill=GREEN)
    d.line(P([(21, 37), (43, 37)]), fill=LIGHT, width=int(1.0 * s))
    # grip
    d.polygon(P([(29, 41), (35, 41), (34, 48), (30, 48)]), fill=DARK)
    d.line(P([(31, 42), (31, 47)]), fill=GREEN, width=int(1.0 * s))
    # pommel
    d.ellipse(P([(29, 47), (35, 53)]), fill=GREEN)


def block(d, s):
    def P(points):
        return [(x * s, y * s) for x, y in points]
    # shield silhouette
    shield = P([(32, 13), (45, 18), (44, 33), (41, 41), (32, 51), (23, 41), (20, 33), (19, 18)])
    d.polygon(shield, fill=GREEN)
    # inner negative-space shield
    inner = P([(32, 19), (40, 22), (39, 32), (37, 38), (32, 45), (27, 38), (25, 32), (24, 22)])
    d.polygon(inner, fill=DARK)
    # leaf vein highlight inside the inner shield
    d.line(P([(32, 22), (32, 42)]), fill=GREEN, width=int(1.2 * s))
    d.line(P([(32, 30), (37, 26)]), fill=GREEN, width=int(1.0 * s))
    d.line(P([(32, 36), (27, 32)]), fill=GREEN, width=int(1.0 * s))
    # left rim highlight
    d.line(P([(22, 21), (21, 32), (25, 39)]), fill=LIGHT, width=int(1.0 * s))


glyph(attack).save(DST + 'attack.png')
glyph(block).save(DST + 'block.png')
print('written')
