package root

import (
	Entities "afterschool.studio/entities"
)

//set value on a differnt line
enemy : Entities.#Sprite
enemy:Renderer:Path: "enemy.png"

//set in single line
ally: Entities.#Sprite & { Renderer:Path: "ally.png" }

//set on multiple lines with embedded data
ally2: Entities.#Sprite & { 
    Position:X: 1
    Renderer:Path: "ally.png"
} & { customData : "some custom data"}