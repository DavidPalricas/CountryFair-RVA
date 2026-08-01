import { Text } from "@react-three/drei";
// Same font as the 2D UI (declared in index.css); troika takes the .ttf directly.
import fontUrl from "../assets/font/Carnevalee Freakshow.ttf";

type Text3DProps = {
    children: string;
    /** Position in the parent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians; the default faces +Z, which is the camera side. */
    rotation?: [number, number, number];
    /** Cap height in world units, not pixels. */
    fontSize?: number;
    /** Width in world units at which the text wraps. */
    maxWidth?: number;
    textAlign?: "left" | "right" | "center" | "justify";
    anchorX?: "left" | "center" | "right" | number;
    anchorY?: "top" | "top-baseline" | "middle" | "bottom-baseline" | "bottom" | number;
    color?: string;
    /** Outline thickness in world units; the default is tuned for the fair orange on grass. */
    outlineWidth?: number;
    outlineColor?: string;
};

/**
 * Shared 3D label used by every sign in the fair (tent names, ribbon numbers, placeholder
 * numbers, the on-screen instruction).
 *
 * Centralising it is what keeps the fairground font and the orange-on-dark-brown colour
 * scheme identical across all of them; callers normally override only position and size.
 */
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
