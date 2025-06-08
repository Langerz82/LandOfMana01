// CustomAnim - Copyright 2024 Joshua Langley.
// Replaces Animations in the AnimationController with ones that match the 
// AnimationClip count. For this to work the AnimationClip files need to be
// name in a certain format, and the clip count needs to match.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

using Animation = UnityEngine.U2D.Animation;
using Debug = UnityEngine.Debug;

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}

public class CustomAnim : MonoBehaviour
{
    [Header("AnimationClipPaths")]
    public string animClipPath = "Entities/Monsters/anim/{0}Frames/{1}";
    public string animClipNameStrip = "bq_monster_";

    [Header("Animations")]
    protected Animator animator;
    protected Animation.SpriteLibrary spriteLibrary;
    protected AnimatorOverrideController animatorOverrideController;
    protected AnimationClipOverrides clipOverrides;

    protected Dictionary<string,int> animCounts = new Dictionary<string,int>();
    protected Dictionary<AnimationClip, AnimationClip> animClips = new Dictionary<AnimationClip, AnimationClip>();
    void Start()
    {
        // Set the defaults.
        animator = GetComponent<Animator>();
        spriteLibrary = GetComponent<Animation.SpriteLibrary>();

        // Generate Animator Override Controller.
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        // Generate Clip Overrides.
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        // Build the animation categories and there respective counts.
        foreach (var category in spriteLibrary.spriteLibraryAsset.GetCategoryNames())
        {
            IEnumerable<string> Labels = spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(category);
            animCounts.Add(category, Labels.Count());
            Debug.Log("CustomAnim - category: " + category + ", count: " + Labels.Count());
        }

        // Add left manually as it is not in the SpriteLibrary.
        // Duplicate values as they will be the same amount just flipped horizontally.
        if (animCounts.ContainsKey("idle_right"))
            animCounts.Add("idle_left", animCounts["idle_right"]);
        if (animCounts.ContainsKey("walk_right"))
            animCounts.Add("walk_left", animCounts["walk_right"]);
        if (animCounts.ContainsKey("attack_right"))
            animCounts.Add("attack_left", animCounts["attack_right"]);

        // Generate the dictionary for new animationClips.
        foreach (var clipov in clipOverrides)
        {
            string category = clipov.Key.name.Replace(animClipNameStrip, "");
            string filename = String.Format(animClipPath, animCounts[category], clipov.Key.name);
            Debug.Log("filename=" + filename);
            AnimationClip animClip = Instantiate(Resources.Load<AnimationClip>(filename));
            if (animClip == null)
            {
                Debug.LogError("animClip filename missing: " + filename);
                continue;
            }
            animClips.Add(clipov.Key, animClip);
        }

        // Override the clip in the default Animation Controller.
        foreach (var aclip in animClips)
        {
            clipOverrides[aclip.Key.name] = aclip.Value;
        }

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
