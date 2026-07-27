type TentBgProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    dimensions?: [number, number, number];
    
};


export function TentBg({ position, rotation = [0, 0, 0], dimensions = [1, 1, 1] }: TentBgProps) {
    return (
        <mesh position={position} rotation={rotation} scale={1}>
            <boxGeometry args={dimensions} />
            <meshStandardMaterial color="black" />
        </mesh>
    
    );
}