using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using VRM;
using UnityEngine.Playables;


public class FileDragAndDrop : MonoBehaviour
{
    // important to keep the instance alive while the hook is active.
    UnityDragAndDropHook hook;
    [SerializeField] Animator move_root;
    [SerializeField] Animator vrm_root;
    [SerializeField] PlayableDirector time_line;
    void OnEnable ()
    {
        // must be created on the main thread to get the right thread id.
        hook = new UnityDragAndDropHook();
        hook.InstallHook();
        hook.OnDroppedFiles += OnFiles;
        var binding = time_line.playableAsset.outputs.First (c => c.streamName == "MoveRoot");
        time_line.SetGenericBinding(binding.sourceObject,move_root);
        SetTimeline();
    }
    void OnDisable()
    {
        hook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        // do something with the dropped file names. aPos will contain the 
        // mouse position within the window where the files has been dropped.
        Debug.Log("Dropped "+aFiles.Count+" files at: " + aPos + "\n"+
            aFiles.Aggregate((a, b) => a + "\n" + b));
        
        string path0 = aFiles[0]; 
            
        try{
            byte[] bytes = System.IO.File.ReadAllBytes(path0);
            var tempVRM = VRMImporter.LoadFromBytes(bytes);
            tempVRM.transform.SetParent(move_root.transform);
            tempVRM.transform.position = vrm_root.transform.position;
            tempVRM.transform.rotation = vrm_root.transform.rotation;
            Destroy(vrm_root.gameObject);
            vrm_root = tempVRM.GetComponent<Animator>() as Animator;
            SetTimeline();
        }catch{
            Debug.Log("MissingRoad");
            return;
        }
    }

    void SetTimeline(){
        var binding = time_line.playableAsset.outputs.First (c => c.streamName == "VRMRoot");
        time_line.SetGenericBinding(binding.sourceObject,vrm_root);
    }
}
