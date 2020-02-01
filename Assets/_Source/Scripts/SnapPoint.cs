using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    /* snap point'in kabul ettiği component tipleri inspector'dan veriliyor */
    [SerializeField] ShipComponentType[] _acceptedComponentTypes;

    public SnapPoint Snapped { get; private set; }      // bu snap point hangi snap point ile bağlı (null ise boşta)

    public ShipComponent Parent { get; private set; }   // bu snap point'in bağlı olduğu component ne? (awake'de alındı)

    public Vector3 Offset { get; private set; }         // snap point'in merkezden uzaklığı (awake'de alındı)

    void Awake()
    {
        Parent = GetComponentInParent<ShipComponent>();
        Offset = transform.localPosition;
    }

    public void Snap(SnapPoint target, bool changeTransform)
    {
        Snapped = target;
        //  pozisyonu ayarlanacak component'in snap point'iysem
        if (changeTransform)
        {
            // öteki snap point de bana bağlı olsun
            target.Snap(this, false);

            // ve hesaplar...
            //Parent.transform.position = target.transform.position - Offset;
            /*
            float targetFace = Mathf.Atan2(target.Offset.z, target.Offset.x);
            float myFutureFace = targetFace + Mathf.PI;
            float myCurrentFace = Mathf.Atan2(Offset.z, Offset.x);

            Quaternion futureFaceQuaternion = Quaternion.Euler(0f, myFutureFace * Mathf.Rad2Deg, 0f);
            Quaternion currentFaceQuaternion = Quaternion.Euler(0f, myCurrentFace * Mathf.Rad2Deg, 0f);

            float angle = Quaternion.Angle(futureFaceQuaternion, currentFaceQuaternion);

            Parent.transform.Rotate(0f, angle - target.Parent.transform.eulerAngles.y, 0f);

            targetFace += target.transform.eulerAngles.y * Mathf.Deg2Rad;
            Parent.transform.position = target.transform.position + new Vector3(Mathf.Abs(target.Offset.x) * Mathf.Cos(targetFace), 0f, Mathf.Abs(target.Offset.z) * Mathf.Sin(targetFace));
            */
            Parent.transform.position = target.transform.position;
        }
    }

    public void Unsnap()
    {
        Snapped = null;
    }

    /* bu snap point parametre gelen tipi kabul ediyor mu? */
    /* sonradan eklenen kontrol aynı zamanda boş mu kontrolu */
    public bool IsAvailable(ShipComponentType type)
    {
        if (Snapped != null)
        {
            return false;
        }

        foreach (ShipComponentType sct in _acceptedComponentTypes)
        {
            if (sct == type)
            {
                return true;
            }
        }
        return false;
    }
}
