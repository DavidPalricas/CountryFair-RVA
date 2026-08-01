import { useLayoutEffect, useMemo, useRef } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { PropertyBinding, type Group } from "three";
import { clone as skeletonClone } from "three/examples/jsm/utils/SkeletonUtils.js";

/** Expects public/models/miniGamesProps/Frisbee/Dog.glb, with the idle clip below. */
const DOG_MODEL = "/models/miniGamesProps/Frisbee/Dog.glb";

/** Expects public/models/miniGamesProps/Frisbee/Frisbee.glb. */
const FRISBEE_MODEL = "/models/miniGamesProps/Frisbee/Frisbee.glb";

/* Clip name exactly as it comes out of the .glb — the typo ("Animatiom") is in the file
   exported from Blender, not here. The other clip is "Armature.001Action". */
const DOG_IDLE_CLIP = "IdleAnimatiom";

/*
  The head bone — the Unity project's "Head", where the frisbee is parented to "End", a child
  of Head. This .glb has neither of those names: only FrontSpine, BackSpine and Root survived
  the export, and End (a leaf bone, weighted to no vertex) was not exported at all. What is
  left is FrontSpine's only child, which is this one. Following the head instead of the End
  gives the same motion: End is rigid inside Head and has no animation channels of its own.

  The sanitizeNodeName call is not decorative: GLTFLoader runs every name through it and it
  eats the dots, so in the loaded scene this bone is called "Leg006" and a
  getObjectByName("Leg.006") would find nothing. We keep the Blender name here and let three
  do the conversion.
*/
const DOG_HEAD_BONE = PropertyBinding.sanitizeNodeName("Leg.006");


type FrisbeeProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Non-uniform scale, unlike the other mini-game props. */
    scale?: [number, number, number];
};

/**
 * Display of the frisbee mini-game in front of its tent: the dog looping its idle animation
 * with the frisbee held in its mouth.
 *
 * The frisbee is not a child of the dog in the scene graph but attached to the head bone at
 * mount, so it follows the animation instead of staying put relative to the body.
 */
export function FrisbeeProps({ position, rotation = [0, 0, 0], scale = [1, 1, 1] }: FrisbeeProps) {
    const { scene: dogScene, animations: dogAnimations } = useGLTF(DOG_MODEL);
    const { scene: frisbeeScene } = useGLTF(FRISBEE_MODEL);

    const groupRef = useRef<Group>(null);

    // The dog is skinned: scene.clone() does not rebind the bones and every copy collapsed
    // into the same position. SkeletonUtils.clone fixes that.
    const dog = useMemo(() => skeletonClone(dogScene), [dogScene]);
    const frisbee = useMemo(() => frisbeeScene.clone(), [frisbeeScene]);

    // The root is the clone and not the original scene: each instance needs its own mixer
    // bound to its own bones, otherwise the animation only took on one of them.
    const { actions, mixer } = useAnimations(dogAnimations, dog);

    /*
      Starting the idle and hanging the frisbee on the dog live in the same effect because the
      order between the two matters (see the mixer.update(0) below).

      The animation moves the bones and not the model's root node, so parenting the frisbee to
      `dog` left it motionless relative to the body — it has to be attached to the head bone.
      attach() (unlike add()) preserves the world transform, so the frisbee stays where its
      position prop put it and only starts following the bone from then on, with no need to
      convert the offset into bone space by hand.
    */
    useLayoutEffect(() => {
        const group = groupRef.current;
        const idle = actions[DOG_IDLE_CLIP];

        if (group === null) {
            return;
        }

        if (!idle) {
            console.warn(`FrisbeeProps: clip "${DOG_IDLE_CLIP}" não existe em ${DOG_MODEL}.`);
            return;
        }

        idle.reset().play();

        /*
          Evaluate frame 0 of the idle BEFORE measuring the frisbee's offset. Without this the
          offset is measured in the bind pose, which is not a pose the dog is ever seen in: the
          frisbee ended up ~0.07 below the spot it had been eyeballed into.
        */
        mixer.update(0);

        const head = dog.getObjectByName(DOG_HEAD_BONE);

        if (head === undefined) {
            console.warn(`FrisbeeProps: bone "${DOG_HEAD_BONE}" não existe em ${DOG_MODEL}.`);
            return () => { idle.stop(); };
        }

        head.attach(frisbee);

        return () => {
            /* Hand the frisbee back to the group so React unmounts it from where it mounted it. */
            group.attach(frisbee);
            idle.stop();
        };
    }, [actions, mixer, dog, frisbee]);

    return (
        <group ref={groupRef} position={position} rotation={rotation} scale={scale}>
            <primitive object={dog} position={[0, 0.1, 0]} scale={0.13} rotation={[0, -Math.PI / 1.5, 0]} />
            <primitive object={frisbee} position={[-0.18, 0.28, 0.18]} scale={0.03} rotation={[0,0, Math.PI / 4]} />
        </group>
    );
}


useGLTF.preload(DOG_MODEL);
useGLTF.preload(FRISBEE_MODEL);
