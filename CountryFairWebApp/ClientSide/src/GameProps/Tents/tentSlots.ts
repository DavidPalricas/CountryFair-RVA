/*
  O analogo dos transforms dos TentPlaceHolder do Unity: a unica fonte de verdade
  das posicoes das tendas. A rotacao pertence ao SLOT e nao a tenda, por isso ao
  trocar de slot a tenda adopta a rotacao de la.
*/

export type MiniGameType = "fishing" | "archery" | "frisbee" | "duckgame";

export const TENT_NAMES: Record<MiniGameType, string> = {
    fishing: "Tenda da Pesca",
    archery: "Tenda de Arco e Flecha",
    frisbee: "Tenda do Frisbee",
    duckgame: "Tenda do Jogo dos Patos",
};

export type TentSlot = {
    number: number;
    position: readonly [number, number, number];
    rotation: readonly [number, number, number];
};

export const TENT_SLOTS: readonly TentSlot[] = [
    { number: 1, position: [-4.2, 0.05, 13], rotation: [0, Math.PI / 6, 0] },
    { number: 2, position: [-1.6, 0.05, 13], rotation: [0, Math.PI / 12, 0] },
    { number: 3, position: [1.0, 0.05, 13], rotation: [0, Math.PI / 200, 0] },
    { number: 4, position: [3.6, 0.05, 13], rotation: [0, -Math.PI / 8, 0] },
];

/* Altura a que as tendas assentam; e tambem o plano onde o arrasto e projectado. */
export const TENT_Y = 0.05;

/*
  Raio em que uma largada conta como sendo dentro do slot. Tem de ser menor que
  metade do espacamento entre slots (2.6) para existir uma zona morta entre eles,
  onde largar devolve a tenda ao slot de origem.
*/
export const DROP_RADIUS = 1.0;

/* Elevacao em Y da tenda enquanto esta a ser arrastada. */
export const DRAG_LIFT = 0.4;
