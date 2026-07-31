import { Text } from "@react-three/drei";
// A mesma fonte do 2D (declarada em index.css); o troika aceita o .ttf directamente.
import fontUrl from "../assets/font/Carnevalee Freakshow.ttf";

type Text3DProps = {
    children: string;
    position: [number, number, number];
    rotation?: [number, number, number];
    fontSize?: number;
    maxWidth?: number;
    textAlign?: "left" | "right" | "center" | "justify";
    anchorX?: "left" | "center" | "right" | number;
    anchorY?: "top" | "top-baseline" | "middle" | "bottom-baseline" | "bottom" | number;
    color?: string;
    outlineWidth?: number;
    outlineColor?: string;
};

export function Text3D({
    children,
    position,
    rotation = [0, 0, 0],
    fontSize = 0.18,
    maxWidth = 1.5,
    textAlign = "center",
    anchorX = "center",
    anchorY = "bottom",
    color = "#ff9700",
    outlineWidth = 0.012,
    outlineColor = "#3a2413",
}: Text3DProps) {
    return (
        <Text
            font={fontUrl}
            position={position}
            rotation={rotation}
            fontSize={fontSize}
            maxWidth={maxWidth}
            textAlign={textAlign}
            anchorX={anchorX}
            anchorY={anchorY}
            color={color}
            outlineWidth={outlineWidth}
            outlineColor={outlineColor}
        >
            {children}
        </Text>
    );
}
