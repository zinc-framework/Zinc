package entities

import (
	Components "afterschool.studio/components"
)

#Sprite : #Entity & {
    Renderer : Components.#SpriteRenderer
    Position : Components.#Position
}