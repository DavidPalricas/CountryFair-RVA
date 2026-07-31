/*
  Substitui o OrderableTentElement do Unity: composicao (hook) em vez de heranca.

  Nao usa o onPointerMove do R3F numa mesh porque durante o arrasto a propria tenda
  tapa o chao e as outras estao invisiveis (logo, fora do raycast). Em vez disso
  captura o ponteiro no canvas e projecta o raio da camara contra um plano matematico
  a altura das tendas.
*/

import { useCallback, useEffect, useLayoutEffect, useRef, type RefObject } from "react";
import { useThree, type ThreeEvent } from "@react-three/fiber";
import { Plane, Raycaster, Vector2, Vector3, type Object3D } from "three";
import { TENT_Y } from "./tentSlots";

type UseTentDragOptions = {
    /* O objecto arrastado; a sua posicao actual define o offset de agarre. */
    objectRef: RefObject<Object3D | null>;
    /*
      Vector3 mutavel onde o ponto de arrasto e escrito. Deliberadamente NAO e estado:
      muda a cada pointermove e um setState por movimento re-renderizava a cena inteira.
      Vem de fora porque os TentPlaceHolder tambem o leem, para saberem qual deles esta
      sob o ponteiro.
    */
    dragPoint: RefObject<Vector3>;
    onDragStart: () => void;
    onDragEnd: (x: number, z: number) => void;
};

export function useTentDrag({ objectRef, dragPoint, onDragStart, onDragEnd }: UseTentDragOptions) {
    const { camera, gl } = useThree();

    /* Diferenca entre a origem da tenda e o ponto onde o rato lhe pegou: sem isto a
       tenda salta para debaixo do cursor no primeiro frame. */
    const grabOffset = useRef(new Vector3());

    const activePointer = useRef<number | null>(null);

    /* Reutilizados entre eventos para nao alocar em cada pointermove. */
    const raycaster = useRef(new Raycaster());
    const pointerNdc = useRef(new Vector2());
    const groundPlane = useRef(new Plane(new Vector3(0, 1, 0), -TENT_Y));
    const groundHit = useRef(new Vector3());

    /*
      Os listeners nativos sao registados uma vez e vivem enquanto o componente existir,
      por isso as callbacks tem de ser lidas de um ref, senao ficavam presas aos valores
      do render em que foram registadas.
    */
    const handlers = useRef({ onDragStart, onDragEnd });

    useLayoutEffect(() => {
        handlers.current = { onDragStart, onDragEnd };
    });

    /* Ecra -> NDC -> raio da camara -> ponto no plano do chao. */
    const projectToGround = useCallback((event: PointerEvent) => {
        const bounds = gl.domElement.getBoundingClientRect();

        pointerNdc.current.set(
            ((event.clientX - bounds.left) / bounds.width) * 2 - 1,
            -((event.clientY - bounds.top) / bounds.height) * 2 + 1,
        );

        raycaster.current.setFromCamera(pointerNdc.current, camera);

        /* Devolve null quando o raio e paralelo ao chao (camara ao nivel do plano). */
        return raycaster.current.ray.intersectPlane(groundPlane.current, groundHit.current);
    }, [camera, gl]);

    useEffect(() => {
        const canvas = gl.domElement;

        const handlePointerMove = (event: PointerEvent) => {
            if (activePointer.current !== event.pointerId) {
                return;
            }

            const ground = projectToGround(event);

            if (ground === null) {
                return;
            }

            dragPoint.current.copy(ground).add(grabOffset.current);
        };

        /*
          Serve pointerup e pointercancel: o cancel e o que garante que o arrasto acaba
          quando o gesto e roubado pelo browser (scroll, gesto do sistema), em vez de
          ficar uma tenda colada ao ponteiro.
        */
        const handlePointerEnd = (event: PointerEvent) => {
            if (activePointer.current !== event.pointerId) {
                return;
            }

            activePointer.current = null;

            if (canvas.hasPointerCapture(event.pointerId)) {
                canvas.releasePointerCapture(event.pointerId);
            }

            handlers.current.onDragEnd(dragPoint.current.x, dragPoint.current.z);
        };

        canvas.addEventListener("pointermove", handlePointerMove);
        canvas.addEventListener("pointerup", handlePointerEnd);
        canvas.addEventListener("pointercancel", handlePointerEnd);

        return () => {
            canvas.removeEventListener("pointermove", handlePointerMove);
            canvas.removeEventListener("pointerup", handlePointerEnd);
            canvas.removeEventListener("pointercancel", handlePointerEnd);
        };
    }, [dragPoint, gl, projectToGround]);

    const onPointerDown = useCallback((event: ThreeEvent<PointerEvent>) => {
        const tent = objectRef.current;

        if (tent === null || activePointer.current !== null) {
            return;
        }

        /* Sem isto as tendas atras desta na fila tambem comecavam a ser arrastadas. */
        event.stopPropagation();

        const ground = projectToGround(event.nativeEvent);

        if (ground === null) {
            return;
        }

        activePointer.current = event.nativeEvent.pointerId;

        /* A captura mantem os eventos a chegar mesmo com o cursor fora do canvas. */
        gl.domElement.setPointerCapture(event.nativeEvent.pointerId);

        grabOffset.current.copy(tent.position).sub(ground);
        dragPoint.current.copy(tent.position);

        handlers.current.onDragStart();
    }, [dragPoint, gl, objectRef, projectToGround]);

    return { onPointerDown };
}
