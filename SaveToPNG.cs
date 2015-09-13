using UnityEngine;
using System.Collections;

// 指定したフレームを任意の解像度でPNGに出力するスクリプト for Unity5
// ## メモ: 
//   * モーション付きMMDモデルの再生画像を高解像度でキャプチャして動画を作ります．
//   * ゲームの解像度と関係なく，指定した解像度の画像を生成できます．
//   * モーション再生のような自動再生されるようなシーンでしか使えないです．キャプチャしない間のフレームはすごい早く動いちゃいます．
//   * どの解像度までいけるかは不明です．GPUが確保できるテクスチャサイズに依存するはず．
// ##  使い方:
// 1. カメラにアタッチする
// 2. 各種設定: 開始フレーム番号(StartFrame)と終了フレーム番号(EndFrame)を指定
// 3. 再生する
public class SaveToPNG : MonoBehaviour
{
    #region　設定
    // 再生時のフレームレート(固定)
    public int FrameRate = 30;
    
    public int StartFrame = 0;
    public int EndFrame = 0;    // 0 : all frame
    
    // 保存先．相対パスの場合はプロジェクトフォルダ直下になるはず（自動フォルダ作成）
    public string SaveFolder = "ScreenShot";
    // ファイル名の接頭語
    public string FileNamePrefix = "S";
    
    // 出力解像度
    public int Width = 640;
    public int Height = 480;
    #endregion

    RenderTexture renderTexture;
    Texture2D texForScreenShot;
    Camera curCamera;

    // Use this for initialization
    void Start()
    {
        Time.captureFramerate =
            FrameRate;  // 固定フレームにしてキャプチャ時間を気にせずゲーム時間を進めるための設定．
        System.IO.Directory.CreateDirectory(SaveFolder);

        curCamera = GetComponent<Camera>();

        renderTexture = new RenderTexture(Width, Height, 24);
        // renderTexture.antiAliasing = 2;  // TODO: なぜかアンチエイリアスを有効にするとうまくキャプチャできない…
        texForScreenShot = new Texture2D(Width, Height, TextureFormat.RGB24, false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var curFrame = Time.frameCount;
        if (StartFrame <= curFrame && (curFrame <= EndFrame || EndFrame == 0)) {
            StartCoroutine(SaveScreenShot());
        }
    }

    void OnDestroy()
    {
        Destroy(renderTexture);
    }

    public IEnumerator SaveScreenShot()
    {
        yield return new WaitForEndOfFrame();

        var prevRT = curCamera.targetTexture;

        curCamera.targetTexture = renderTexture;
        curCamera.Render();
        RenderTexture.active = renderTexture;
        texForScreenShot.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        texForScreenShot.Apply();

        curCamera.targetTexture = precRT;
        RenderTexture.active = null;

        // Encode texture into PNG
        byte[] bytes = texForScreenShot.EncodeToPNG();

        // For testing purposes, also write to a file in the project folder
        string fileName = string.Format("{0}{1:D04}.png", FileNamePrefix, Time.frameCount);
        var path = System.IO.Path.Combine(SaveFolder, fileName);

        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved: " + path);
    }
}
