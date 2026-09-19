using UnityEngine;

public class ARModelRegisterar : MonoBehaviour
{
    public Transform baseModel;
    public Transform clothModel;
    public Transform drinksContainer;

    private void Awake()
    {
        // 在第0帧时强制隐藏所有渲染节点，防止在 ARImageSmoothFollow 瞬移过程中的残影闪现
        if (baseModel!= null) baseModel.gameObject.SetActive(false);
        if (clothModel!= null) clothModel.gameObject.SetActive(false);
        if (drinksContainer!= null) drinksContainer.gameObject.SetActive(false);
    }
    private void Start()
    {
        // 当MenuRoot(Clone)被实例化时，主动寻找全局管理器并注册自己的子节点引用
        if (CocktailARManager.Instance!= null)
        {
            CocktailARManager.Instance.RegisterDynamicReferences(this.gameObject,baseModel, clothModel, drinksContainer);
        }
    }
}