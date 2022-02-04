# Shaders

## Sprites

### Base

A sprite uber shader.

This is built upon the Unity 2020 sprite shader code. It includes Photoshop-style blend mode functionality and other bells and whistles.

The intended use is that you build your own materials using this shader. However, the package comes with a few ready-made materials to act as presets:

#### Additive

The sprite is drawn to the screen using an additive blend mode.

#### Additive Tint

The tint color is applied to the sprite using an additive blend mode.

#### Color Replace

This shader variation searches for color A throughout the sprite and replaces it with color B.

#### Duochrome

This shader variation expects a black and white sprite. Blacks are replaced with color A; whites are replaced with color B.

If we encounter a gray, then we linearly interpolate between A and B.

If the sprite is in color, then we only use the red channel and ignore blue and gree.

#### Multiply

The sprite is drawn to the screen using a multiply blend mode.

#### Overlay Tint

The tint color is applied to the sprite using an overlay blend mode.

#### Stroke

An outline is drawn around opaque parts of the sprite.

**Note:** For this to work properly, the sprite image needs to have a padding of transparent pixels. The outline will not extend beyond the bounds of the overall sprite.

## Screen

Screen effect shaders to be used in post-processing.

### Fade

Simultaneously darken and desaturate the screen.

### Pixelate

Pixelate the screen.
