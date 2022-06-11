# Shaders

## Common

### Sprite

A sprite uber shader.

This is built upon the Unity 2020 sprite shader code. It includes Photoshop-style blend mode functionality and other bells and whistles.

The intended use is that you build your own materials using this shader. However, the package comes with a few ready-made materials to act as presets.

#### Sprite Blend Mode

The blend mode for the entire sprite.

#### Tint Blend Mode

The blend mode for the tint color on the sprite graphic.

#### Color Replace

Find color A throughout the sprite and replace it with color B. The higher the fuzziness value, the more similar-to-A colors will also be replaced.

#### Duochrome

This function expects a black and white sprite. Blacks are replaced with color A; whites are replaced with color B.

If we encounter a gray, then we linearly interpolate between A and B.

If the sprite is in color, then we only use the red channel and ignore blue and green.

#### Stroke

An outline is drawn around opaque parts of the sprite.

**Note:** For this to work properly, the sprite image needs to have a padding of transparent pixels. The outline will not extend beyond the bounds of the overall sprite.

### Surface

A standard surface shader with extra capabilities.

#### Outline

An outline is drawn around the object.

### Retone

Replace the colors and lighting on an object with textures. Choose a texture based on the underlying color value.

### Water

A simple water shader. Includes foam at shore's edge, opacity by depth, and animated an caustics texture.

## Screen

### Fade

Simultaneously darken and desaturate the screen.

### Fog

Set distant objects to a specified color.

### Pixelate

Pixelate the screen.

### Retone

Replace the colors on the screen with textures. Choose a texture based on the underlying color value.

### Outline

Draw outlines based on screen normals and depth. Based on the outline screen effect at https://www.ronja-tutorials.com.
