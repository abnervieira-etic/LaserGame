using UnityEngine;
public class RangedWeapon : BaseGadget
{
    [Header("Referências")]
    [SerializeField] private UnityEngine.Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = UnityEngine.Camera.main;
    }

    private void Update()
    {
        // Só a parte visual (viewmodel seguindo a câmera) roda todo frame.
        // A ativação (atirar) NÃO é lida aqui — quem manda atirar é o
        // GadgetHolder, chamando UseGadget() (herdado da base).
        AlignWeaponToCamera();
    }

    // Chamado pelo UseGadget() da base, só depois do cooldown já ter
    // sido verificado. Não precisa (e não deve) checar Input aqui.
    protected override void ActivateGadget()
    {
        RaycastShoot();
    }

    private void AlignWeaponToCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;
        }

        
        // Sem o "-" que tinha antes: LookRotation(-forward) apontava a arma
        // pra TRÁS da câmera. O certo é só copiar a rotação da câmera.
        transform.rotation = mainCamera.transform.rotation;
    }

    private void RaycastShoot()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Agora usando gadgetData.range de verdade, em vez de raycast infinito.
        if (Physics.Raycast(ray, out RaycastHit hit, gadgetData.range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Exemplo opcional: se você tiver uma interface de dano no projeto,
            // é aqui que gadgetData.damage entraria em jogo. Se ainda não tem,
            // pode ignorar/remover esse bloco por enquanto.
            /*
            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(gadgetData.damage);
            }
            /*/
        }
    }
}