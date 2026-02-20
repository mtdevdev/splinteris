using UnityEngine;
using Unity.Cinemachine; 
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Base Settings")]
    public Vector3 baseOffset = new Vector3(0, 6, 0);

    [Header("Dynamic Settings")]
    public float maxCameraShift = 3f; 
    public float maxMouseDistance = 10f;
    
    [Header("Smoothing")]
    public float movementSmoothTime = 0.3f; 
    public float inputSmoothTime = 0.1f; 

    [Header("Collision")]
    public LayerMask collisionLayer; 
    public float collisionRadius = 0.3f; 

    [Header("Mouse Raycast")]
    public LayerMask groundLayer;

    private CinemachineCamera vcam;
    private CinemachineFollow follow;
    
    private Vector3 currentVelocity; 
    private Vector3 currentMouseWorldPos;
    private Vector3 mouseVelocity;

    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        follow = vcam.GetComponent<CinemachineFollow>();
        
        if (player != null) currentMouseWorldPos = player.position;
    }

    private void LateUpdate()
    {
        if (player == null || follow == null) return;

        // 1. Detectar posição do Mouse (Alvo Bruto)
        Vector3 targetMousePos = player.position; // Default caso raycast falhe
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            targetMousePos = hit.point;
        }

        // Se o jogador mover o mouse super rápido, 'currentMouseWorldPos' viaja suavemente até lá
        currentMouseWorldPos = Vector3.SmoothDamp(currentMouseWorldPos, targetMousePos, ref mouseVelocity, inputSmoothTime);

        // 2. Calcular direção e influência baseada no Input Suavizado
        Vector3 directionToMouse = currentMouseWorldPos - player.position;
        directionToMouse.y = 0f;

        float currentDistance = directionToMouse.magnitude;
        float influence = Mathf.Clamp01(currentDistance / maxMouseDistance);

        Vector3 targetShift = Vector3.zero;
        if (currentDistance > 0.01f)
        {
            targetShift = directionToMouse.normalized * (maxCameraShift * influence);
        }

        // 3. Calcular onde a câmera DESEJA estar (Ideal Position)
        // O Offset final desejado relativo ao player
        Vector3 desiredOffset = baseOffset + targetShift;
        // A posição no mundo onde a câmera ficaria
        Vector3 desiredCameraWorldPos = player.position + desiredOffset;

        // FIX PAREDES: Verificar colisão entre o Player e a Câmera
        Vector3 finalOffset = desiredOffset;
        
        // Direção do player para a câmera
        Vector3 dirPlayerToCam = desiredCameraWorldPos - player.position;
        float distPlayerToCam = dirPlayerToCam.magnitude;

        // SphereCast é como um Raycast, mas "grosso", para evitar que a camera entre em buracos pequenos
        if (Physics.SphereCast(player.position, collisionRadius, dirPlayerToCam.normalized, out RaycastHit wallHit, distPlayerToCam, collisionLayer))
        {
            // Se bateu na parede, colocamos a câmera um pouco antes da parede
            // O hit.distance é a distância do player até a parede
            // Subtraímos o collisionRadius para não ficar colado na textura
            float safeDistance = Mathf.Max(0.5f, wallHit.distance - collisionRadius);
            
            // Recalculamos o offset baseado nessa nova distância segura
            finalOffset = dirPlayerToCam.normalized * safeDistance;
            
            // Opcional: Manter a altura original se preferir que ela só dê zoom in
            finalOffset.y = baseOffset.y; 
        }

        // 4. Aplicação Final Suave
        // Usamos SmoothDamp novamente aqui para garantir que a correção de colisão também seja suave
        Vector3 smoothedOffset = Vector3.SmoothDamp(follow.FollowOffset, finalOffset, ref currentVelocity, movementSmoothTime);
        follow.FollowOffset = smoothedOffset;
    }
    
    // Debug visual para ver a colisão
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Vector3 camPos = player.position + baseOffset;
            if (follow != null) camPos = player.position + follow.FollowOffset;
            
            Gizmos.DrawLine(player.position, camPos);
            Gizmos.DrawWireSphere(camPos, collisionRadius);
        }
    }
}