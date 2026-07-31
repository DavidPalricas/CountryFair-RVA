import { useEffect, useLayoutEffect, useRef, useState, type RefObject } from "react";
import { useFrame } from "@react-three/fiber";
import { Euler, MathUtils, Quaternion, type Group, type Vector3 } from "three";
import {Tent} from "./Tent";
import {ArcheryProps} from "../MiniGamesProps/ArcheryProps";
import { FrisbeeProps } from "../MiniGamesProps/FrisbeeProps";
import { FishingProps } from "../MiniGamesProps/FishingProps";
import { DuckProps } from "../MiniGamesProps/DuckProps";
import { Text3D } from "../Text3D";
import {TentRibbon} from "./TentRibbon";
import { DRAG_LIFT, TENT_NAMES, TENT_SLOTS, TENT_Y, type MiniGameType } from "./tentSlots";
import { useTentDrag } from "./useTentDrag";

/* Multiplicador da escala base enquanto o ponteiro esta em cima da tenda. */
const HOVER_SCALE = 1.2;

/* Amortecimento exponencial das animacoes: mais alto = chega mais depressa ao alvo. */
const DAMPING = 12;

/*
  Temporarios ao nivel do modulo em vez de alocados dentro do useFrame. O JavaScript
  e single-threaded e nenhum deles e guardado depois de usado, por isso partilha-los
  entre as tendas e seguro e evita lixo a cada frame.
*/
const targetEuler = new Euler();
const targetQuaternion = new Quaternion();

type MiniGameTentProps = {
    type: MiniGameType;
    slot: number;
    isDragging: boolean;
    /* ElementSelected() do Unity: as tendas que nao estao a ser arrastadas escondem-se. */
    hidden: boolean;
    /* Partilhado com os TentPlaceHolder, que o leem para saber qual esta sob o ponteiro. */
    dragPoint: RefObject<Vector3>;
    onDragStart: (type: MiniGameType) => void;
    onDragEnd: (type: MiniGameType, x: number, z: number) => void;
};


export function MiniGameTent({ type, slot, isDragging, hidden, dragPoint, onDragStart, onDragEnd }: MiniGameTentProps) {
    const miniGamePropsPos: [number, number, number] = [0.6, 0, 2];
    const groupRef = useRef<Group>(null);
    const [hovered, setHovered] = useState(false);

    const { onPointerDown } = useTentDrag({
        objectRef: groupRef,
        dragPoint,
        onDragStart: () => onDragStart(type),
        onDragEnd: (x, z) => onDragEnd(type, x, z),
    });

    const slotTransform = TENT_SLOTS[slot];

    /*
      Posicao e rotacao passam a ser escritas no useFrame, por isso nao vem por prop no
      <group>: o transform inicial e posto uma vez no mount e a animacao toma conta dele
      a partir dai. As dependencias sao deliberadamente vazias — quando o slot muda
      queremos a transicao animada, nao um salto.
    */
    useLayoutEffect(() => {
        const group = groupRef.current;

        if (group === null) {
            return;
        }

        group.position.set(...slotTransform.position);
        group.rotation.set(...slotTransform.rotation);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    /*
      Uma tenda escondida sai do raycast do three, por isso nunca chega a receber o
      onPointerOut e ficaria presa em hover (com o cursor agarrado) ate voltar a aparecer.
    */
    useEffect(() => {
        if (hidden || isDragging) {
            setHovered(false);
        }
    }, [hidden, isDragging]);

    /* O cursor e reposto na limpeza do efeito e nao no onPointerOut, senao ficaria
       preso caso a tenda desmonte ou se esconda enquanto esta em hover. */
    useEffect(() => {
        if (!hovered) {
            return;
        }

        document.body.style.cursor = "grab";
        return () => { document.body.style.cursor = "auto"; };
    }, [hovered]);

    useFrame((_, delta) => {
        const group = groupRef.current;

        if (group === null) {
            return;
        }

        const step = 1 - Math.exp(-DAMPING * delta);

        if (isDragging) {
            /* Segue o ponteiro em XZ, com uma elevacao fixa em Y para a tenda ler como
               "levantada" em vez de raspar o chao. */
            group.position.set(dragPoint.current.x, TENT_Y + DRAG_LIFT, dragPoint.current.z);
        } else {
            /* SnapToCurrentPlaceHolder(), mas interpolado em vez de instantaneo. */
            group.position.x = MathUtils.lerp(group.position.x, slotTransform.position[0], step);
            group.position.y = MathUtils.lerp(group.position.y, slotTransform.position[1], step);
            group.position.z = MathUtils.lerp(group.position.z, slotTransform.position[2], step);

            /* Slerp de quaternioes em vez de interpolar angulos de Euler: evita a tenda
               dar a volta pelo lado errado quando a diferenca passa os 180 graus. */
            targetEuler.set(...slotTransform.rotation);
            targetQuaternion.setFromEuler(targetEuler);
            group.quaternion.slerp(targetQuaternion, step);
        }

        const targetScale = hovered && !isDragging ? HOVER_SCALE : 1;
        group.scale.setScalar(MathUtils.lerp(group.scale.x, targetScale, step));
    });

    return (
        <group
            ref={groupRef}
            name="MiniGameTent"
            /* O three ignora objectos invisiveis no raycast, por isso isto reproduz o
               SetActive(false) do Unity, incluindo deixarem de receber eventos. */
            visible={!hidden}
            onPointerDown={onPointerDown}
            /* O R3F entrega o evento a todas as interseccoes ao longo do raio, por isso sem
               o stopPropagation as tendas que ficam atras desta tambem entravam em hover. */
            onPointerOver={(event) => { event.stopPropagation(); setHovered(true); }}
            onPointerOut={() => setHovered(false)}
        >
            {/* Letreiro e fita escondem-se durante o arrasto, como o Unity esconde o
                tentText/tentNumber/ribbon enquanto a tenda esta agarrada. */}
            <group visible={!isDragging}>
                {/* A tenda tem 1.58 de altura, por isso o letreiro assenta em 1.7.
                    maxWidth mantem o texto dentro da largura da tenda (1.63). */}
                <Text3D position={[0, 1.7, 0]}>
                    {TENT_NAMES[type]}
                </Text3D>

                {/* O numero vem do slot e nao da tenda, tal como o SetTentNumber() do Unity
                    o le do currentPlaceHolder: ao trocar de slot a tenda adopta o numero de la. */}
                <TentRibbon position={[-1.2, 1.7, 0]} number={slotTransform.number}/>
            </group>

            <Tent position={[0, 0, 0]}/>
            {type === "fishing" && <FishingProps position={miniGamePropsPos} />}
            {type === "archery" && <ArcheryProps position={miniGamePropsPos} />}
            {type === "frisbee" && <FrisbeeProps position={miniGamePropsPos} />}
            {type === "duckgame" && <DuckProps position={miniGamePropsPos} />}
        </group>
    );
}
