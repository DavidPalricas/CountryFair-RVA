/**
 * Pure ordering logic: no React and no Three, so it can be tested without mounting a scene
 * and wired to Colyseus later without touching the components. It is the equivalent of
 * Unity's `PlaceHolderManager.UpdateElementPosition()`.
 */

import { DROP_RADIUS, TENT_SLOTS, type MiniGameType } from "./tentSlots";

/** The array index is the slot index: `order[2]` is the tent occupying slot 3. */
export type TentOrder = MiniGameType[];

/**
 * Returns the index of the slot whose centre is within DROP_RADIUS in X of the point, or
 * null when the drop landed in the dead zone between slots. This replaces the Unity
 * collider `OnTriggerEnter`/`Exit` pair, which does not exist here because R3F brings no
 * physics.
 *
 * Only X is considered because the drag is locked to the row line (see `useTentDrag`): all
 * slots share the same Z, so the Z distance is always zero.
 */
export function slotIndexAtX(x: number): number | null {
    for (let slot = 0; slot < TENT_SLOTS.length; slot++) {
        if (Math.abs(x - TENT_SLOTS[slot].position[0]) <= DROP_RADIUS) {
            return slot;
        }
    }

    return null;
}

/**
 * Swaps `tent` with whoever occupies `targetSlot`.
 *
 * Returns the SAME reference when nothing changes, so React's setState does not trigger a
 * needless render. An unknown tent or an out-of-range slot is a no-op for the same reason.
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
