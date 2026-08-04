using UnityEngine;
using UnityEngine.UI;

public class UISetting : MonoBehaviour
{
    public Image targetUIImage;
    public Material hdrMaterialPrefab;

    [ColorUsage(true, true)]
    public Color myHDRColor;

    private static readonly int _hdrColorID = Shader.PropertyToID("_HDRColor");

    private Material instancedMaterial;

    private void Start()
    {
        instancedMaterial = new Material(hdrMaterialPrefab);
        targetUIImage.material = instancedMaterial;

        instancedMaterial.SetColor(_hdrColorID, myHDRColor);
    }

    public void ChangeToNeonRed()
    {
        //색깔 바꾸고싶으면 호출
    }
}
