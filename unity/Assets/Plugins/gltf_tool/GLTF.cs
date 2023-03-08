using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GLTFast;
using GLTFast.Export;

public class GLTF : MonoBehaviour
{
    public static void AdvancedExport(MemoryStream stream) {

        // CollectingLogger lets you programatically go through
        // errors and warnings the export raised
        var logger = new CollectingLogger();

        // ExportSettings allow you to configure the export
        // Check its source for details
        var exportSettings = new ExportSettings {
            format = GltfFormat.Binary,
            fileConflictResolution = FileConflictResolution.Overwrite
        };

        // GameObjectExport lets you create glTFs from GameObject hierarchies
        var export = new GameObjectExport( exportSettings, logger: logger);

        // Example of gathering GameObjects to be exported (recursively)
        var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportGLTF");
        // TODO set this or add as input

        // Add a scene
        export.AddScene(rootLevelNodes, "Canvas");

        // create memory stream for output
        export.SaveToStreamAndDispose(stream);

        // print size of stream
        Debug.Log("Stream size: " + stream.Length);

        // if(!success) {
        //     Debug.LogError("Something went wrong exporting a glTF");
        //     // Log all exporter messages
        //     logger.LogAll();
        // }
    }
}
