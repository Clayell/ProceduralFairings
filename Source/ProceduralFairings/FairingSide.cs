//  ==================================================
//  Procedural Fairings plug-in by Alexey Volynskov.

//  Licensed under CC-BY-4.0 terms: https://creativecommons.org/licenses/by/4.0/legalcode
//  ==================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keramzit
{
    public class ProceduralFairingSide : PartModule, IPartCostModifier, IPartMassModifier
    {
        [KSPField] public float minBaseConeAngle = 20;
        [KSPField] public float colliderShaveAngle = 5;
        [KSPField] public Vector4 baseConeShape = new Vector4 (0, 0, 0, 0);
        [KSPField] public Vector4 noseConeShape = new Vector4 (0, 0, 0, 0);

        [KSPField] public Vector2 mappingScale = new Vector2 (1024, 1024);
        [KSPField] public Vector2 stripMapping = new Vector2 (992, 1024);
        [KSPField] public Vector4 horMapping = new Vector4 (0, 480, 512, 992);
        [KSPField] public Vector4 vertMapping = new Vector4 (0, 160, 704, 1024);

        [KSPField] public float costPerTonne = 2000;
        [KSPField] public float specificBreakingForce = 2000;
        [KSPField] public float specificBreakingTorque = 2000;

        public float defaultBaseCurveStartX;
        public float defaultBaseCurveStartY;
        public float defaultBaseCurveEndX;
        public float defaultBaseCurveEndY;
        public float defaultBaseConeSegments;

        public float defaultNoseCurveStartX;
        public float defaultNoseCurveStartY;
        public float defaultNoseCurveEndX;
        public float defaultNoseCurveEndY;
        public float defaultNoseConeSegments;
        public float defaultNoseHeightRatio;

        public float totalMass;

        [KSPField (isPersistant = true)] public int numSegs = 12;
        [KSPField (isPersistant = true)] public int numSideParts = 2;
        [KSPField (isPersistant = true)] public float baseRad;
        [KSPField (isPersistant = true)] public float maxRad = 1.50f;
        [KSPField (isPersistant = true)] public float cylStart = 0.5f;
        [KSPField (isPersistant = true)] public float cylEnd = 2.5f;
        [KSPField (isPersistant = true)] public float topRad;
        [KSPField (isPersistant = true)] public float inlineHeight;
        [KSPField (isPersistant = true)] public float sideThickness = 0.05f;
        [KSPField (isPersistant = true)] public Vector3 meshPos = Vector3.zero;
        [KSPField (isPersistant = true)] public Quaternion meshRot = Quaternion.identity;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Base Auto-shape")]
        [UI_Toggle (disabledText = "Off", enabledText = "On")]
        public bool baseAutoShape = true;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Base Curve Point A", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float baseCurveStartX = 0.5f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Base Curve Point B", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float baseCurveStartY = 0.0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Base Curve Point C", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float baseCurveEndX = 1.0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Base Curve Point D", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float baseCurveEndY = 0.5f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Base Cone Segments")]
        [UI_FloatRange (minValue = 1, maxValue = 12, stepIncrement = 1)]
        public float baseConeSegments = 5;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Nose Auto-shape")]
        [UI_Toggle (disabledText = "Off", enabledText = "On")]
        public bool noseAutoShape = true;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Nose Curve Point A", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float noseCurveStartX = 0.5f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Nose Curve Point B", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float noseCurveStartY = 0.0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Nose Curve Point C", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float noseCurveEndX = 1.0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Nose Curve Point D", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.0f, maxValue = 1.0f, incrementLarge = 0.1f, incrementSmall = 0.01f, incrementSlide = 0.01f)]
        public float noseCurveEndY = 0.5f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Nose Cone Segments")]
        [UI_FloatRange (minValue = 1, maxValue = 12, stepIncrement = 1)]
        public float noseConeSegments = 7;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Nose-height Ratio", guiFormat = "S4")]
        [UI_FloatEdit (sigFigs = 2, minValue = 0.1f, maxValue = 5.0f, incrementLarge = 1.0f, incrementSmall = 0.1f, incrementSlide = 0.01f)]
        public float noseHeightRatio = 2.0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Shape")]
        [UI_Toggle (disabledText = "Unlocked", enabledText = "Locked")]
        public bool shapeLock;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiName = "Density")]
        [UI_FloatRange (minValue = 0.01f, maxValue = 1.0f, stepIncrement = 0.01f)]
        public float density = 0.2f;

        [KSPField (isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Mass")]
        public string massDisplay;

        [KSPField (isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Cost")]
        public string costDisplay;

        public ModifierChangeWhen GetModuleCostChangeWhen () { return ModifierChangeWhen.FIXED; }
        public ModifierChangeWhen GetModuleMassChangeWhen () { return ModifierChangeWhen.FIXED; }

        public float GetModuleCost (float defcost, ModifierStagingSituation sit)
        {
            return totalMass * costPerTonne - defcost;
        }

        public float GetModuleMass (float defmass, ModifierStagingSituation sit)
        {
            return totalMass - defmass;
        }

        public override string GetInfo ()
        {
            const string infoString = "Attach to a procedural fairing base to reshape. Right-click it to set it's parameters.";

            return infoString;
        }

        public void Start ()
        {
            part.mass = totalMass;
        }

        public override void OnStart (StartState state)
        {
            if (state == StartState.None)
            {
                return;
            }

            if (state != StartState.Editor || shapeLock)
            {
                rebuildMesh ();
            }

            //  Set the initial fairing side curve values from the part config.

            baseCurveStartX = baseConeShape.x;
            baseCurveStartY = baseConeShape.y;
            baseCurveEndX   = baseConeShape.z;
            baseCurveEndY   = baseConeShape.w;

            noseCurveStartX = noseConeShape.x;
            noseCurveStartY = noseConeShape.y;
            noseCurveEndX   = noseConeShape.z;
            noseCurveEndY   = noseConeShape.w;

            //  Save the default fairing side values for later use.

            defaultBaseCurveStartX  = baseCurveStartX;
            defaultBaseCurveStartY  = baseCurveStartY;
            defaultBaseCurveEndX    = baseCurveEndX;
            defaultBaseCurveEndY    = baseCurveEndY;
            defaultBaseConeSegments = baseConeSegments;

            defaultNoseCurveStartX  = noseCurveStartX;
            defaultNoseCurveStartY  = noseCurveStartY;
            defaultNoseCurveEndX    = noseCurveEndX;
            defaultNoseCurveEndY    = noseCurveEndY;
            defaultNoseConeSegments = noseConeSegments;
            defaultNoseHeightRatio  = noseHeightRatio;

            //  Set the initial fairing side mass value.

            part.mass = totalMass;

            //  Set up the GUI update callbacks.

            OnUpdateFairingSideUI ();

            OnToggleFairingShapeUI ();
        }

        public override void OnLoad (ConfigNode cfg)
        {
            base.OnLoad (cfg);

            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                rebuildMesh ();
            }
        }

        void OnUpdateFairingSideUI ()
        {
            ((UI_Toggle) Fields["baseAutoShape"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_Toggle) Fields["noseAutoShape"].uiControlEditor).onFieldChanged += OnChangeShapeUI;

            ((UI_FloatEdit) Fields["baseCurveStartX"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["baseCurveStartY"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["baseCurveEndX"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["baseCurveEndY"].uiControlEditor).onFieldChanged += OnChangeShapeUI;

            ((UI_FloatEdit) Fields["noseCurveStartX"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["noseCurveStartY"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["noseCurveEndX"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["noseCurveEndY"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatEdit) Fields["noseHeightRatio"].uiControlEditor).onFieldChanged += OnChangeShapeUI;

            ((UI_FloatRange) Fields["baseConeSegments"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatRange) Fields["noseConeSegments"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
            ((UI_FloatRange) Fields["density"].uiControlEditor).onFieldChanged += OnChangeShapeUI;
        }

        void OnChangeShapeUI (BaseField bf, object obj)
        {
            //  Set the default values of the fairing side base parameters if the auto-shape is enabled.

            if (baseAutoShape)
            {
                baseCurveStartX  = defaultBaseCurveStartX;
                baseCurveStartY  = defaultBaseCurveStartY;
                baseCurveEndX    = defaultBaseCurveEndX;
                baseCurveEndY    = defaultBaseCurveEndY;
                baseConeSegments = defaultBaseConeSegments;
            }

            //  Set the default values of the fairing side nose parameters if the auto-shape is enabled.

            if (noseAutoShape)
            {
                noseCurveStartX  = defaultNoseCurveStartX;
                noseCurveStartY  = defaultNoseCurveStartY;
                noseCurveEndX    = defaultNoseCurveEndX;
                noseCurveEndY    = defaultNoseCurveEndY;
                noseConeSegments = defaultNoseConeSegments;
                noseHeightRatio  = defaultNoseHeightRatio;
            }

            //  Set the state of the advanced fairing side base and nose options.

            OnToggleFairingShapeUI ();

            //  Update the fairing shape.

            var fairingSide = part.GetComponent<ProceduralFairingBase>();

            if (fairingSide != null)
            {
                //  Rebuild the fairing mesh.

                fairingSide.needShapeUpdate = true;
            }
        }

        void OnToggleFairingShapeUI ()
        {
            Fields["baseCurveStartX"].guiActiveEditor  = !baseAutoShape;
            Fields["baseCurveStartY"].guiActiveEditor  = !baseAutoShape;
            Fields["baseCurveEndX"].guiActiveEditor    = !baseAutoShape;
            Fields["baseCurveEndY"].guiActiveEditor    = !baseAutoShape;
            Fields["baseConeSegments"].guiActiveEditor = !baseAutoShape;

            Fields["noseCurveStartX"].guiActiveEditor  = !noseAutoShape;
            Fields["noseCurveStartY"].guiActiveEditor  = !noseAutoShape;
            Fields["noseCurveEndX"].guiActiveEditor    = !noseAutoShape;
            Fields["noseCurveEndY"].guiActiveEditor    = !noseAutoShape;
            Fields["noseHeightRatio"].guiActiveEditor  = !noseAutoShape;
            Fields["noseConeSegments"].guiActiveEditor = !noseAutoShape;
        }

        public void updateNodeSize ()
        {
            var node = part.FindAttachNode ("connect");

            if (node != null)
            {
                int s = Mathf.RoundToInt (baseRad * 2 / 1.25f) - 1;

                if (s < 0)
                {
                    s = 0;
                }

                node.size = s;
            }
        }

        public void FixedUpdate ()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                int nsym = part.symmetryCounterparts.Count;

                if (nsym == 0)
                {
                    massDisplay = PFUtils.formatMass (totalMass);
                    costDisplay = PFUtils.formatCost (part.partInfo.cost + GetModuleCost (part.partInfo.cost, ModifierStagingSituation.CURRENT));
                }
                else if (nsym == 1)
                {
                    massDisplay = PFUtils.formatMass (totalMass * 2) + " (both)";
                    costDisplay = PFUtils.formatCost ((part.partInfo.cost + GetModuleCost (part.partInfo.cost, ModifierStagingSituation.CURRENT)) * 2) + " (both)";
                }
                else
                {
                    massDisplay = PFUtils.formatMass (totalMass * (nsym + 1)) + " (all " + (nsym + 1) + ")";
                    costDisplay = PFUtils.formatCost ((part.partInfo.cost + GetModuleCost (part.partInfo.cost, ModifierStagingSituation.CURRENT)) * (nsym + 1)) + " (all " + (nsym + 1) + ")";
                }
            }
        }

        // dirs are a list of rotations in the y-plane for normals along the fairside edge
        // they are an alternate description of number of colliders per part.
        // shape[i].x is the radius of the fairingside
        // shape[i].y is the y-coord of point i, the height on the fairingside
        // shape[i].z I have no idea.  It's a lerp between magic numbers in the vertexMapping.
        // presumably this describes curvature somehow
        private void RebuildColliders(Vector3[] shape, Vector3[] dirs)
        {
            if (part.FindModelComponent<MeshFilter>("model") is MeshFilter mf)
            {
                //  Remove any old colliders.
                foreach (Collider c in part.FindModelComponents<Collider>())
                    Destroy(c.gameObject);

                float anglePerPart = Mathf.Max((360f / numSideParts) - colliderShaveAngle, 1);
                int numColliders = dirs.Length;
                float anglePerCollider = anglePerPart / numColliders;
                float startAngle = (-anglePerPart / 2) + (anglePerCollider / 2);
                //  Add the new colliders.
                {
                    //  Nose collider.
                    GameObject obj = new GameObject("nose_collider");
                    SphereCollider coll = obj.AddComponent<SphereCollider>();
                    float r = (inlineHeight > 0) ? sideThickness / 2 : maxRad * 0.2f;
                    float tip = maxRad * noseHeightRatio;
                    float collCenter = (cylStart + cylEnd) / 2;

                    coll.transform.parent = mf.transform;
                    coll.transform.localRotation = Quaternion.identity;
                    coll.transform.localPosition = (inlineHeight > 0) ?
                                                    new Vector3(maxRad + r, collCenter, 0) :
                                                    new Vector3(r, cylEnd + tip - r * 1.2f, 0);
                    coll.center = Vector3.zero;
                    coll.radius = r;
                }

                // build list of normals from shape[], the list of points on the inside surface
                Vector3[] normals = new Vector3[shape.Length];
                for (int i=0; i<shape.Length; i++)
                {
                    Vector3 norm;
                    if (i == 0)
                        norm = cylStart > float.Epsilon ? new Vector3(0, 1, 0) : shape[1] - shape[0];
                    else if (i == shape.Length - 1)
                        norm = new Vector3(0, 1, 0);
                    else
                        norm = shape[i + 1] - shape[i - 1];
                    norm.Set(norm.y, -norm.x, 0);
                    normals[i] = norm.normalized;
                }
                for (int i = 0; i < shape.Length - 1; i++)
                {
                    // p.x, p.y is a point on the 2D shape projection.  p.z == ??
                    // normals[i] is the 3D normal to (p.x, p.y, 0)
                    Vector3 p = shape[i];
                    Vector3 pNext = shape[i + 1];
                    p.z = pNext.z = 0;
                    // Project the points outward and build a grid on the outer edge.
                    p += normals[i] * sideThickness;
                    pNext += normals[i+1] * sideThickness;

                    // Build a grid between pNext and p
                    Vector3 n = pNext - p;
                    n.Set(n.y, -n.x, 0);
                    n.Normalize();
                    // n is normal to the normal-projected shape[i],aligned to x=forward

                    // shape[i] is a list of points along a vertical slice
                    // dirs[j] is a list of normals around a horizontal slice
                    // Create faces/box colliders centered between shape[i],shape[i+1] of desired angular radius
                    // cp is the centerpoint of the collider, positioned 1/10th of sideThickness inside the fairing outer edge.
                    Vector3 cp = (pNext + p) / 2;
                    cp -= (sideThickness * 0.1f) * n;
                    float collWidth = cp.x * Mathf.PI * 2 / (numSideParts * numColliders);
                    Vector3 size = new Vector3(collWidth, (pNext - p).magnitude, sideThickness * 0.1f);
                    // Skip the collider if adjacent points are too close.
                    if (size.y > 0.001)
                        BuildColliderRow(mf.transform, p, cp, n, size, numColliders, startAngle, anglePerCollider, $"{i}");
                }
                /*
                if ((inlineHeight <= 0 && numStandardColliders > 2) || (inlineHeight > 0 && cylEnd < inlineHeight))
                {
                    // Draw the last two colliders on the top and inner face near the last point.
                    // Draw from projection of (k-1)th point to p
                    Vector3 p = shape[shape.Length-1];
                    Vector3 pPrev = shape[shape.Length - 2];
                    p.z = pPrev.z = 0;
                    p += normals[shape.Length-1] * sideThickness;
                    pPrev += normals[shape.Length-2] * sideThickness;
                    // Compute n = normal to (pPrev->p) == normal to surface
                    Vector3 n = p - pPrev;
                    n.Set(n.y, -n.x, 0);
                    n.Normalize();
                    // Get projected point on the outer surface
                    //Vector3 proj = pPrev + (n * sideThickness);
                    //p.x += sideThickness;
                    // Compute normal between projected point and endpoint p
                    //n = p - pPrev;
                    //n.Set(n.y, -n.x, 0);
                    //n.Normalize();

                    Vector3 cp = (pPrev + p) / 2;
                    float collWidth = cp.x * Mathf.PI * 2 / (numSideParts * numColliders);
                    Vector3 size = new Vector3(collWidth, (p - pPrev).magnitude, 0.01f);
                    // cp is currently the midpoint between the last and next-to-last point on the inside of the fairing.
                    // Draw very thin colliders on the inner face.
                    BuildColliderRow(mf.transform, p, cp, n, size, numColliders, startAngle, anglePerCollider, "topInner");
                    // Draw thin collider on inner surface from p-1 to p
                    n = p - pPrev;
                    n.Set(n.y, -n.x, 0);
                    n.Normalize();

                    cp = (pPrev + p) / 2;
                    collWidth = cp.x * Mathf.PI * 2 / (numSideParts * numColliders);
                    size = new Vector3(collWidth, sideThickness, 0.01f);
                    BuildColliderRow(mf.transform, p, cp, n, size, numColliders, startAngle, anglePerCollider, "top");
                }
                */
            }
        }

        private void BuildColliderRow(Transform parent, Vector3 p, Vector3 cp, Vector3 normal, Vector3 size, int numColliders, float startAngle, float anglePerCollider, string name)
        {
            for (int j = 0; j < numColliders; j++)
            {
                float rotAngle = startAngle + (j * anglePerCollider);
                Quaternion RotY = Quaternion.Euler(0, -rotAngle, 0);

                GameObject obj = new GameObject($"collider_{name}_{j}");
                BoxCollider coll = obj.AddComponent<BoxCollider>();
                coll.transform.parent = parent;

                Vector3 projectedP = new Vector3(Mathf.Cos(rotAngle * Mathf.Deg2Rad) * p.x,
                                                            p.y,
                                                            Mathf.Sin(rotAngle * Mathf.Deg2Rad) * p.x);
                Vector3 projectedCP = new Vector3(Mathf.Cos(rotAngle * Mathf.Deg2Rad) * cp.x,
                                                            cp.y,
                                                            Mathf.Sin(rotAngle * Mathf.Deg2Rad) * cp.x);

                // forward = z becomes the direction of the normal; up is direction to the next point
                coll.transform.localPosition = projectedCP;
                coll.transform.localRotation = Quaternion.LookRotation(RotY * normal, (projectedCP - projectedP).normalized);
                coll.center = Vector3.zero;
                coll.size = size;
            }
        }

        public void rebuildMesh ()
        {
            var mf = part.FindModelComponent<MeshFilter>("model");

            if (!mf)
            {
                Debug.LogError ("[PF]: No model for side fairing!", part);

                return;
            }

            Mesh m = mf.mesh;

            if (!m)
            {
                Debug.LogError ("[PF]: No mesh in side fairing model!", part);

                return;
            }

            mf.transform.localPosition = meshPos;
            mf.transform.localRotation = meshRot;

            updateNodeSize ();

            //  Build the fairing shape line.

            float tip = maxRad * noseHeightRatio;

            Vector3 [] shape;

            baseConeShape = new Vector4 (baseCurveStartX, baseCurveStartY, baseCurveEndX, baseCurveEndY);
            noseConeShape = new Vector4 (noseCurveStartX, noseCurveStartY, noseCurveEndX, noseCurveEndY);

            if (inlineHeight <= 0)
            {
                shape = ProceduralFairingBase.buildFairingShape (baseRad, maxRad, cylStart, cylEnd, noseHeightRatio, baseConeShape, noseConeShape, (int) baseConeSegments, (int) noseConeSegments, vertMapping, mappingScale.y);
            }
            else
            {
                shape = ProceduralFairingBase.buildInlineFairingShape (baseRad, maxRad, topRad, cylStart, cylEnd, inlineHeight, baseConeShape, (int) baseConeSegments, vertMapping, mappingScale.y);
            }

            //  Set up parameters.

            var dirs = new Vector3 [numSegs + 1];

            for (int i = 0; i <= numSegs; ++i)
            {
                float a = Mathf.PI * 2 * (i - numSegs * 0.5f) / (numSideParts * numSegs);

                dirs [i] = new Vector3 (Mathf.Cos (a), 0, Mathf.Sin (a));
            }

            float segOMappingScale = (horMapping.y - horMapping.x) / (mappingScale.x * numSegs);
            float segIMappingScale = (horMapping.w - horMapping.z) / (mappingScale.x * numSegs);
            float segOMappingOfs = horMapping.x / mappingScale.x;
            float segIMappingOfs = horMapping.z / mappingScale.x;

            if (numSideParts > 2)
            {
                segOMappingOfs += segOMappingScale * numSegs * (0.5f - 1f / numSideParts);
                segOMappingScale *= 2f / numSideParts;

                segIMappingOfs += segIMappingScale * numSegs * (0.5f - 1f / numSideParts);
                segIMappingScale *= 2f / numSideParts;
            }

            float stripU0 = stripMapping.x / mappingScale.x;
            float stripU1 = stripMapping.y / mappingScale.x;

            float ringSegLen = baseRad * Mathf.PI * 2 / (numSegs * numSideParts);
            float topRingSegLen = topRad * Mathf.PI * 2 / (numSegs * numSideParts);

            int numMainVerts = (numSegs + 1) * (shape.Length - 1) + 1;
            int numMainFaces = numSegs * ((shape.Length - 2) * 2 + 1);

            int numSideVerts = shape.Length * 2;
            int numSideFaces = (shape.Length - 1) * 2;

            int numRingVerts = (numSegs + 1) * 2;
            int numRingFaces = numSegs * 2;

            if (inlineHeight > 0)
            {
                numMainVerts = (numSegs + 1) * shape.Length;
                numMainFaces = numSegs * (shape.Length - 1) * 2;
            }

            int totalVerts = numMainVerts * 2 + numSideVerts * 2 + numRingVerts;
            int totalFaces = numMainFaces * 2 + numSideFaces * 2 + numRingFaces;

            if (inlineHeight > 0)
            {
                totalVerts += numRingVerts;
                totalFaces += numRingFaces;
            }

            var p = shape [shape.Length - 1];

            float topY = p.y, topV = p.z;

            //  Compute the area.

            double area = 0;

            for (int i = 1; i < shape.Length; ++i)
            {
                area += (shape [i - 1].x + shape [i].x) * (shape [i].y - shape [i - 1].y) * Mathf.PI / numSideParts;
            }

            //  Set the parameters based on volume.

            float volume = (float) (area * sideThickness);

            part.mass = totalMass = volume * density;
            part.breakingForce = part.mass * specificBreakingForce;
            part.breakingTorque = part.mass * specificBreakingTorque;

            float anglePerPart = 360f / numSideParts;
            float x = Mathf.Cos(Mathf.Deg2Rad * anglePerPart / 2);
            Vector3 offset = new Vector3(maxRad * (1 + x) / 2, topY * 0.5f, 0);
            part.CoMOffset = part.transform.InverseTransformPoint(mf.transform.TransformPoint(offset));

            RebuildColliders(shape, dirs);

            //  Build the fairing mesh.

            m.Clear ();

            var verts = new Vector3 [totalVerts];
            var uv = new Vector2 [totalVerts];
            var norm = new Vector3 [totalVerts];
            var tang = new Vector4 [totalVerts];

            if (inlineHeight <= 0)
            {
                //  Tip vertex.

                verts [numMainVerts - 1].Set (0, topY + sideThickness, 0);      //  Outside.
                verts [numMainVerts * 2 - 1].Set (0, topY, 0);                  //  Inside.

                uv [numMainVerts - 1].Set (segOMappingScale * 0.5f * numSegs + segOMappingOfs, topV);
                uv [numMainVerts * 2 - 1].Set (segIMappingScale * 0.5f * numSegs + segIMappingOfs, topV);

                norm [numMainVerts - 1] = Vector3.up;
                norm [numMainVerts * 2 - 1] = -Vector3.up;

                tang [numMainVerts - 1] = Vector3.zero;
                tang [numMainVerts * 2 - 1] = Vector3.zero;
            }

            //  Main vertices.

            float noseV0 = vertMapping.z / mappingScale.y;
            float noseV1 = vertMapping.w / mappingScale.y;
            float noseVScale = 1f / (noseV1 - noseV0);
            float oCenter = (horMapping.x + horMapping.y) / (mappingScale.x * 2);
            float iCenter = (horMapping.z + horMapping.w) / (mappingScale.x * 2);

            int vi = 0;

            for (int i = 0; i < shape.Length - (inlineHeight <= 0 ? 1 : 0); ++i)
            {
                p = shape [i];

                Vector2 n;

                if (i == 0)
                {
                    n = shape [1] - shape [0];
                }
                else if (i == shape.Length - 1)
                {
                    n = shape [i] - shape [i - 1];
                }
                else
                {
                    n = shape [i + 1] - shape [i - 1];
                }

                n.Set (n.y, -n.x);

                n.Normalize ();

                for (int j = 0; j <= numSegs; ++j, ++vi)
                {
                    var d = dirs [j];

                    var dp = d * p.x + Vector3.up * p.y;
                    var dn = d * n.x + Vector3.up * n.y;

                    if (i == 0 || i == shape.Length - 1)
                    {
                        verts [vi] = dp + d * sideThickness;
                    }
                    else
                    {
                        verts [vi] = dp + dn * sideThickness;
                    }

                    verts[vi + numMainVerts] = dp;

                    float v = (p.z - noseV0) * noseVScale;
                    float uo = j * segOMappingScale + segOMappingOfs;
                    float ui = (numSegs - j) * segIMappingScale + segIMappingOfs;

                    if (v > 0 && v < 1)
                    {
                        float us = 1 - v;

                        uo = (uo - oCenter) * us + oCenter;
                        ui = (ui - iCenter) * us + iCenter;
                    }

                    uv [vi].Set (uo, p.z);

                    uv [vi + numMainVerts].Set (ui, p.z);

                    norm [vi] = dn;
                    norm [vi + numMainVerts] = -dn;

                    tang [vi].Set (-d.z, 0, d.x, 0);
                    tang [vi + numMainVerts].Set (d.z, 0, -d.x, 0);
                }
            }

            //  Side strip vertices.

            float stripScale = Mathf.Abs (stripMapping.y - stripMapping.x) / (sideThickness * mappingScale.y);

            vi = numMainVerts * 2;

            float o = 0;

            for (int i = 0; i < shape.Length; ++i, vi += 2)
            {
                int si = i * (numSegs + 1);

                var d = dirs [0];

                verts [vi] = verts [si];

                uv [vi].Set (stripU0, o);
                norm [vi].Set (d.z, 0, -d.x);

                verts [vi + 1] = verts [si + numMainVerts];
                uv [vi + 1].Set (stripU1, o);
                norm [vi + 1] = norm[vi];
                tang [vi] = tang [vi + 1] = (verts [vi + 1] - verts [vi]).normalized;

                if (i + 1 < shape.Length)
                {
                    o += ((Vector2) shape [i + 1] - (Vector2) shape [i]).magnitude * stripScale;
                }
            }

            vi += numSideVerts - 2;

            for (int i = shape.Length - 1; i >= 0; --i, vi -= 2)
            {
                int si = i * (numSegs + 1) + numSegs;

                if (i == shape.Length - 1 && inlineHeight <= 0)
                {
                    si = numMainVerts - 1;
                }

                var d = dirs [numSegs];

                verts [vi] = verts [si];
                uv [vi].Set (stripU0, o);
                norm [vi].Set (-d.z, 0, d.x);

                verts [vi + 1] = verts [si + numMainVerts];
                uv [vi + 1].Set (stripU1, o);
                norm [vi + 1] = norm [vi];
                tang [vi] = tang [vi + 1] = (verts [vi + 1] - verts [vi]).normalized;

                if (i > 0)
                {
                    o += ((Vector2) shape [i] - (Vector2) shape [i - 1]).magnitude * stripScale;
                }
            }

            //  Ring vertices.

            vi = numMainVerts * 2 + numSideVerts * 2;

            o = 0;

            for (int j = numSegs; j >= 0; --j, vi += 2, o += ringSegLen * stripScale)
            {
                verts [vi] = verts [j];
                uv [vi].Set (stripU0, o);
                norm [vi] = -Vector3.up;

                verts [vi + 1] = verts [j + numMainVerts];
                uv [vi + 1].Set (stripU1, o);
                norm [vi + 1] = -Vector3.up;
                tang [vi] = tang [vi + 1] = (verts [vi + 1] - verts [vi]).normalized;
            }

            if (inlineHeight > 0)
            {
                //  Top ring vertices.

                o = 0;

                int si = (shape.Length - 1) * (numSegs + 1);

                for (int j = 0; j <= numSegs; ++j, vi += 2, o += topRingSegLen * stripScale)
                {
                    verts [vi] = verts [si + j];
                    uv [vi].Set (stripU0, o);
                    norm [vi] = Vector3.up;

                    verts [vi + 1] = verts [si + j + numMainVerts];
                    uv [vi + 1].Set (stripU1, o);
                    norm [vi + 1] = Vector3.up;
                    tang [vi] = tang [vi + 1] = (verts [vi + 1] - verts [vi]).normalized;
                }
            }

            //  Set vertex data to mesh.

            for (int i = 0; i < totalVerts; ++i)
            {
                tang [i].w = 1;
            }

            m.vertices = verts;
            m.uv = uv;
            m.normals = norm;
            m.tangents = tang;

            m.uv2 = null;
            m.colors32 = null;

            var tri = new int [totalFaces * 3];

            //  Main faces.

            vi = 0;

            int ti1 = 0, ti2 = numMainFaces * 3;

            for (int i = 0; i < shape.Length - (inlineHeight <= 0 ? 2 : 1); ++i, ++vi)
            {
                p = shape [i];

                for (int j = 0; j < numSegs; ++j, ++vi)
                {
                    tri [ti1++] = vi;
                    tri [ti1++] = vi + 1 + numSegs + 1;
                    tri [ti1++] = vi + 1;

                    tri [ti1++] = vi;
                    tri [ti1++] = vi + numSegs + 1;
                    tri [ti1++] = vi + 1 + numSegs + 1;

                    tri [ti2++] = numMainVerts + vi;
                    tri [ti2++] = numMainVerts + vi + 1;
                    tri [ti2++] = numMainVerts + vi + 1 + numSegs + 1;

                    tri [ti2++] = numMainVerts + vi;
                    tri [ti2++] = numMainVerts + vi + 1 + numSegs + 1;
                    tri [ti2++] = numMainVerts + vi + numSegs + 1;
                }
            }

            if (inlineHeight <= 0)
            {
                //  Main tip faces.

                for (int j = 0; j < numSegs; ++j, ++vi)
                {
                    tri [ti1++] = vi;
                    tri [ti1++] = numMainVerts - 1;
                    tri [ti1++] = vi + 1;

                    tri [ti2++] = numMainVerts + vi;
                    tri [ti2++] = numMainVerts + vi + 1;
                    tri [ti2++] = numMainVerts + numMainVerts - 1;
                }
            }

            //  Side strip faces.

            vi = numMainVerts * 2;
            ti1 = numMainFaces * 2 * 3;
            ti2 = ti1 + numSideFaces * 3;

            for (int i = 0; i < shape.Length - 1; ++i, vi += 2)
            {
                tri [ti1++] = vi;
                tri [ti1++] = vi + 1;
                tri [ti1++] = vi + 3;

                tri [ti1++] = vi;
                tri [ti1++] = vi + 3;
                tri [ti1++] = vi + 2;

                tri [ti2++] = numSideVerts + vi;
                tri [ti2++] = numSideVerts + vi + 3;
                tri [ti2++] = numSideVerts + vi + 1;

                tri [ti2++] = numSideVerts + vi;
                tri [ti2++] = numSideVerts + vi + 2;
                tri [ti2++] = numSideVerts + vi + 3;
            }

            //  Ring faces.

            vi = numMainVerts * 2 + numSideVerts * 2;
            ti1 = (numMainFaces + numSideFaces) * 2 * 3;

            for (int j = 0; j < numSegs; ++j, vi += 2)
            {
                tri [ti1++] = vi;
                tri [ti1++] = vi + 1;
                tri [ti1++] = vi + 3;

                tri [ti1++] = vi;
                tri [ti1++] = vi + 3;
                tri [ti1++] = vi + 2;
            }

            if (inlineHeight > 0)
            {
                //  Top ring faces.

                vi += 2;

                for (int j = 0; j < numSegs; ++j, vi += 2)
                {
                    tri [ti1++] = vi;
                    tri [ti1++] = vi + 1;
                    tri [ti1++] = vi + 3;

                    tri [ti1++] = vi;
                    tri [ti1++] = vi + 3;
                    tri [ti1++] = vi + 2;
                }
            }

            m.triangles = tri;

            StartCoroutine (PFUtils.updateDragCubeCoroutine (part, 1));
        }
    }
}
