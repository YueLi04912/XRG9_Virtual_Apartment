using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;

public static class AnnotationExporter
{
    // 获取统一的保存路径 (Quest 3 内置存储或 Windows 的 AppData 目录)
    public static string GetExportFolder()
    {
        string path = Path.Combine(Application.persistentDataPath, "AnnotationData");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    // 1. 将 AudioClip 转换并保存为真实的 WAV 文件
    public static string SaveAudioToWav(AudioClip clip, int markerId)
    {
        string folder = GetExportFolder();
        string fileName = $"Marker_{markerId}_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string filePath = Path.Combine(folder, fileName);

        // 提取音频 PCM 数据
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        Int16[] intData = new Int16[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
        }

        // 写入标准的 WAV 文件头和数据
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + intData.Length * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16); // 16 bit
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(intData.Length * 2);
            foreach (var sample in intData) writer.Write(sample);
        }
        return fileName;
    }

    // 2. 将所有小球数据更新到 CSV 表格中
    // 2. 在游戏结束时统一调用，按 Session 导出带时间戳的 CSV
    // 导出最终的 CSV 报表（在游戏关闭时调用）
    public static void ExportSessionCSV()
    {
        string folder = GetExportFolder();
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = System.IO.Path.Combine(folder, $"AnnotationReport_{timestamp}.csv");

        // 查找场景中所有活着的标记小球
        FeedbackMarker[] allMarkers = UnityEngine.Object.FindObjectsOfType<FeedbackMarker>();

        // 写入 CSV（使用 UTF8 编码防止中文乱码）
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, new System.Text.UTF8Encoding(true)))
        {
            // 【修改点 1】：在表头增加第 7 列 “语音识别文本”
            sw.WriteLine("ID,Priority,PositionX,PositionY,PositionZ,AudioFiles,Transcriptions");

            foreach (var marker in allMarkers)
            {
                // 用 | 符号连接同一个小球的多段音频
                string audioList = string.Join(" | ", marker.audioFiles);
                
                // 【修改点 2】：用 | 符号连接同一个小球的多段 AI 识别文字
                string textList = string.Join(" | ", marker.transcribedTexts);
                
                Vector3 pos = marker.transform.position;
                
                // 【修改点】：在 {textList} 两边加上转义的双引号 \" 
                sw.WriteLine($"{marker.markerId},{marker.priorityText},{pos.x:F3},{pos.y:F3},{pos.z:F3},{audioList},\"{textList}\"");
            }
        }
        
        Debug.Log($"[Data Management] 报表已成功生成，共包含 {allMarkers.Length} 个标记: {filePath}");
    }
    // 3. 物理删除硬盘上的废弃音频文件
    public static void DeleteAudioFiles(System.Collections.Generic.List<string> fileNames)
    {
        string folder = GetExportFolder();
        foreach (var fileName in fileNames)
        {
            string filePath = Path.Combine(folder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        Debug.Log($"[Data Management] Cleaned up {fileNames.Count} deleted audio files from disk.");
    }
}