
/** The four mini-games that own a tent; mirrors Unity's `MINI_GAMES` enum. */
export type MiniGameType = "fishing" | "archery" | "frisbee" | "duckgame";

/** Portuguese tent names shown on the signboard above each tent. */
export const TENT_NAMES: Record<MiniGameType, string> = {
    fishing: "Tenda da Pesca",
    archery: "Tenda de Arco e Flecha",
    frisbee: "Tenda do Frisbee",
    duckgame: "Tenda do Jogo dos Patos",
};

/** One position in the row a tent can occupy. */
export type TentSlot = {
    /** Slot label shown on the ribbon, 1-based (the array index is 0-based). */
    number: number;
    /** World-space position of the slot centre. */
    position: readonly [number, number, number];
    /** Euler rotation in radians. */
    rotation: readonly [number, number, number];
};


/** Height the tents rest at; slightly above 0 so they sit on the grass box, not inside it. */
export const TENT_Y = 0.05;


/** Camera position: roughly standing eye height, in front of the row. */
export const VIEWER_POSITION: [number, number, number] = [0, 1.8, 17.6];


/** Distance in X between two slot centres. */
const SLOT_SPACING = 3.4;

/**
 * The row sits ~8.6 units in front of the camera. It used to be at 4.6, and at that distance
 * the outer tents fell almost 50 degrees off the camera axis, where perspective distorts them
 * most. Pushing the row back puts all of them within ~31 degrees, and at this distance the
 * whole row fits on screen with a 60 fov even in a narrow (4:3) window.
 */
export const TENT_ROW_Z = 9;

/** Slot centres in multiples of SLOT_SPACING, symmetric about x = 0. */
const SLOT_OFFSETS = [-1.5, -0.5, 0.5, 1.5];


/**
 * Every tent shares the same rotation: the row only reads as aligned when the tents are
 * parallel to each other. Each one used to be turned to face the player, which at the ends
 * came to almost 50 degrees and left the first and last tents facing one another instead of
 * lining up.
 */
const SLOT_ROTATION: readonly [number, number, number] = [0, 0, 0];

/** The four slots, left to right; index in this array is the slot index used by `TentOrder`. */
export const TENT_SLOTS: readonly TentSlot[] = SLOT_OFFSETS.map((offset, index): TentSlot => ({
    number: index + 1,
    position: [offset * SLOT_SPACING, TENT_Y, TENT_ROW_Z],
    rotation: SLOT_ROTATION,
}));


/**
 * How close in X a drop must land to count as being in a slot. At 0.4 of the spacing the
 * catch areas cover most of the row but stay disjoint, so a drop can never match two slots.
 */
export const DROP_RADIUS = SLOT_SPACING * 0.4;

/** How high a tent floats above TENT_Y while dragged, so it reads as picked up. */
export const DRAG_LIFT = 0.4;
