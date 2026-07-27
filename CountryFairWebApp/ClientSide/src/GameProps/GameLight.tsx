type LightProps = {
    position: [number, number, number];
    color?: string;
    intensity?: number;
};

export function GameLight({ position, color, intensity }: LightProps) {
    return (
        <directionalLight position={position} color={color} intensity={intensity} />
    );
}