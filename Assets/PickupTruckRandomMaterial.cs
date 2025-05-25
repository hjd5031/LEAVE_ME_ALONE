using UnityEngine;

public class PickupTruckRandomMaterial : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    public Material[] materials;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (materials != null && materials.Length > 0)
        {
            int index = Random.Range(0, materials.Length);
            _meshRenderer.material = materials[index];
        }
        else
        {
            Debug.LogWarning("Material 배열이 비어 있습니다.");
        }
    }
}