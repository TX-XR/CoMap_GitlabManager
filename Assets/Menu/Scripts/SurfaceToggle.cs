using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MappingAI
{
    /// <summary>Changes the surface type</summary>
    public class SurfaceToggle : MonoBehaviour
    {
        [SerializeField] private Button previous;
        [SerializeField] private Button next;
        [SerializeField] private Text value;

        void Start()
        {
            previous.onClick.AddListener(UpdateSurfaceType);
            next.onClick.AddListener(UpdateSurfaceType);

            UpdateText();
        }

        void UpdateSurfaceType()
        {
            //switch (ApplicationSettings.Instance.surfaceType)
            //{
            //    case SurfaceType.DrawingSurface:
            //        ApplicationSettings.Instance.surfaceType = SurfaceType.DeskPivot;
            //        break;
            //    case SurfaceType.DeskPivot:
            //        ApplicationSettings.Instance.surfaceType = SurfaceType.DrawingSurface;
            //        break;
            //}

            //if (ApplicationSettings.Instance.forceTextureSurface)
            //    ApplicationSettings.Instance.surfaceType = SurfaceType.DrawingSurface;

            //if (EventSystem.current)
            //    EventSystem.current.SetSelectedGameObject(null);

            UpdateText();
        }

        void UpdateText()
        {
            //switch (ApplicationSettings.Instance.surfaceType)
            //{
            //    case SurfaceType.DrawingSurface:
            //        value.text = "DrawingSurface";
            //        break;
            //    case SurfaceType.DeskPivot:
            //        value.text = "DeskPivot";
            //        break;
            //}
        }
    }
}