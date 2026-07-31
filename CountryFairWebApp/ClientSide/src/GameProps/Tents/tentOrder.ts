/*
  Logica de ordenacao pura: sem React e sem Three, para poder ser testada sem montar
  cena nenhuma e para ser ligada ao Colyseus mais tarde sem mexer nos componentes.
  E o equivalente ao PlaceHolderManager.UpdateElementPosition() do Unity.
*/

import { DROP_RADIUS, TENT_SLOTS, type MiniGameType } from "./tentSlots";

/* O indice do array e o indice do slot: order[2] e a tenda que ocupa o slot 3. */
export type TentOrder = MiniGameType[];

/*
  Devolve o indice do slot cujo centro esta a menos de DROP_RADIUS do ponto, ou null
  se a largada caiu na zona morta entre slots. Substitui os OnTriggerEnter/Exit dos
  colliders do Unity, que aqui nao existem porque o R3F nao traz fisica.
*/
export function slotIndexAtPoint(x: number, z: number): number | null {
    for (let slot = 0; slot < TENT_SLOTS.length; slot++) {
        const [slotX, , slotZ] = TENT_SLOTS[slot].position;
        const deltaX = x - slotX;
        const deltaZ = z - slotZ;

        /* Comparacao ao quadrado para evitar a raiz quadrada. */
        if (deltaX * deltaX + deltaZ * deltaZ <= DROP_RADIUS * DROP_RADIUS) {
            return slot;
        }
    }

    return null;
}

/*
  Troca a tenda com quem ocupa targetSlot. Devolve a MESMA referencia quando nao ha
  mudanca, para o setState do React nao disparar um render desnecessario.
*/
export function swapIntoSlot(order: TentOrder, tent: MiniGameType, targetSlot: number): TentOrder {
    const currentSlot = order.indexOf(tent);

    if (currentSlot === -1 || targetSlot < 0 || targetSlot >= order.length) {
        return order;
    }

    if (currentSlot === targetSlot) {
        return order;
    }

    const swapped = [...order];
    swapped[currentSlot] = order[targetSlot];
    swapped[targetSlot] = tent;

    return swapped;
}
