Gabriel session compile probe (VaM mono / DynamicCSharp crash isolation)
===========================================================================

Your logs show an access violation inside mono.dll during script compilation:
SyncPluginUrl -> CompileFiles -> TypeBuilder.CreateType ->
mono_custom_attrs_get_attr. These cslists rebuild the SAME sources as
GabrielSessionPlugins.cslist, but in controlled slices so you can move the crash
boundary.

Paths are relative to this folder:
  Custom/Scripts/Gabriel/session-plugins-compile-probe/

HOW TO RUN A PROBE INSTEAD OF THE NORMAL SESSION BUNDLE
-------------------------------------------------------

1. In VaM CoreControl Plugin Manager, edit the MAIN session plugin URL entry
   (GabrielBootstrap injects plugin#1), OR edit bootstrap once:

   Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs

   Change the sessionPlugins entry from:
     Custom/Scripts/Gabriel/session-plugins/GabrielSessionPlugins.cslist

   To one probe path below, for example:
     Custom/Scripts/Gabriel/session-plugins-compile-probe/stageA00_stub_only.cslist

   Keep VaMLogClipboardHud.cslist as-is unless you are isolating clipboard too.

2. Prefer a CLEAN CoreControl preset (empty plugin slots) once so Bootstrap can
   LateRestoreFromJSON probe URLs reliably; stale compiled assemblies can hide
   the real trigger.

PATH A — suspects first (clothing strip dependency chain + stub host)
---------------------------------------------------------------------

Each stage is CUMULATIVE with GabrielCompileProbeHost.cs so VaM always has one
MVRScript in the compile batch.

Order reflects likely suspects tied to TriggerClothingRemover:

  stageA00_stub_only.cslist
  stageA01_stub_PersonAtomCache.cslist
  stageA02_stub_Classifier.cslist
  stageA03_stub_VrInput.cslist          (UnityEngine.XR reads)
  stageA04_stub_TriggerStrip.cslist     (Male2 hands + garment APIs)
  stageA05_stub_strip_plus_SceneSettleRuntime.cslist
                                        (+ SceneSettle runtime partials only)

Interpretation:

  Crash appears on A03 -> prioritize VrInput / XR typeref paths.
  Crash appears on A02 -> prioritize ClothingClassifier emission.
  Crash appears on A01 -> prioritize PersonAtomCache.
  A00 works but A04 crashes -> full strip stack is implicated.
  A passes all the way through -> move to Path B or subtractive experiments.

PATH B — production file order, cumulative (no stub host)
---------------------------------------------------------

These mirror session-plugins/GabrielSessionPlugins.cslist with paths rewritten
from this folder (see REFERENCE_full_as_session_plugins_probe.cslist).

There is NO GabrielCompileProbeHost: the first MVRScript appears once
GabrielHud / GabrielSessionOrchestrator / DildoOnHands enter the batch.

Early stageB_accum*.cslist batches may report normal C# compile *errors* until
enough cross-references exist — that is OK. The mono access violation is the
target signature you are hunting.

  stageB_accum4.cslist    first 4 paths  (SceneSettle trio + Orchestrator)
  stageB_accum8.cslist    first 8 paths
  stageB_accum12.cslist   first 12 paths
  stageB_accum16.cslist   first 16 paths
  stageB_accum20.cslist   first 20 paths
  stageB_accum24.cslist   first 24 paths
  stageB_accum28.cslist   first 28 paths
  stageB_accum32.cslist   first 32 paths
  stageB_accum37.cslist   full reference (should match production surface)

REFERENCE_full_as_session_plugins_probe.cslist should be line-for-line
equivalent to the production cslist (37 sources), only path-prefixed for this
directory.

FAST SUBTRACTION EXPERIMENTS (full bundle minus slices)
-------------------------------------------------------

Baseline is REFERENCE_full_as_session_plugins_probe.cslist.

  experiment_no_clothing_strip.cslist
    Drops ClothingClassifier + TriggerClothingRemover only.

  experiment_no_dildo.cslist
    Drops DildoOnHands only.

  experiment_no_clothing_and_no_dildo.cslist
    Drops both strips above.

Binary chop on file index (expect second half alone to FAIL compile):

  experiment_first_half_files_18.cslist   produces first 18 production paths.
  experiment_second_half_files_19.cslist    last 19 production paths alone.

RULES WHEN EDITING THESE LISTS
------------------------------

Never leave blank lines inside a cslist row list (VaM treats an empty row as a
path and can blow up oddly).

Prefer forward slashes like the shipped Gabriel cslist.

When you rename or move Gabriel sources later, regenerate Path B manifests from
session-plugins/GabrielSessionPlugins.cslist.
