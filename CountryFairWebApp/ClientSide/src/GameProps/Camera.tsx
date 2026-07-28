import { useThree } from "@react-three/fiber";
import { useLayoutEffect, useRef } from "react";
import type { PerspectiveCamera as PerspectiveCameraImpl } from "three";

type CameraProps = {
    position: [number, number, number];
    lookAt?: [number, number, number];
    fov?: number;
    near?: number;
    far?: number;
};


export function Camera({ position, lookAt = [0, 0, 0], fov = 120, near = 0.1, far = 1000 }: CameraProps) {
    const ref = useRef<PerspectiveCameraImpl>(null!);
    const set = useThree((state) => state.set);

    // Sem isto o Canvas renderiza com a sua camara default e esta e apenas mais um objeto na cena
    useLayoutEffect(() => {
        set({ camera: ref.current });
    }, [set]);

    useLayoutEffect(() => {
        ref.current.lookAt(lookAt[0], lookAt[1], lookAt[2]);
        ref.current.updateProjectionMatrix();
    }, [position[0], position[1], position[2], lookAt[0], lookAt[1], lookAt[2], fov, near, far]);

    return (
        <perspectiveCamera ref={ref} position={position} fov={fov} near={near} far={far} />
    );
}
