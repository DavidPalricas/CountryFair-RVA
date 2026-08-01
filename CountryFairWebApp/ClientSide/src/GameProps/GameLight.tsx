type LightProps = {
    /** Direction the light comes from, as a world-space position; it always aims at the origin. */
    position: [number, number, number];
    /** Defaults to a warm daylight tint. */
    color?: string;
    intensity?: number;
};

/**
 * Directional light used as the fair's sun and as its cool fill; see `GameScreen` for how the
 * two instances are placed against each other.
 */
export function GameLight({ position, color = "#fff4d6", intensity = 2.2 }: LightProps) {
    return (
        <directionalLight position={position} color={color} intensity={intensity} />
    );
}
