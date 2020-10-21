using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 3;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public float m_BossIntroDelay = 3f;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private WaitForSeconds m_BossIntroWait;
    private TankManager m_RoundWinner;
    private TankManager m_RealRoundWinner;
    private TankManager m_GameWinner;

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_BossIntroWait = new WaitForSeconds(m_BossIntroDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        int playerCount = 0;
        int CPUCount = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            if (!m_Tanks[i].m_isCPU)
            {
                playerCount++;
                m_Tanks[i].m_PlayerNumber = playerCount;
            }
            else
            {
                CPUCount++;
                m_Tanks[i].m_CPUNumber = CPUCount;
            }
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;

        for (int i = 0; i < targets.Length; i++)
        {
            m_Tanks[i].m_Instance.GetComponent<TankMovement>().m_Targets = targets;
        }
    }
    /*
    public ArrayList<int> findPlayerIndices()
    {
        ArrayList<int> playerIndices = new ArrayList<int>();
        for(int i = 0; i < targets.Length; i++)
        {
            if (!m_Tanks[i].m_isCPU)
            {
                playerIndices.Add(i);
            }
        }

        return playerIndices;
    }
    */

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            Application.Quit();
            //SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        while (GetLastEnemyIndex() == -1 && m_Tanks[1].m_Instance.activeSelf)
        {
            yield return null;
        }

        if (GetLastEnemyIndex() != -2 && m_Tanks[1].m_Instance.activeSelf)
        {
            DisableTankControl();

            m_Tanks[GetLastEnemyIndex()].m_Instance.GetComponent<TankMovement>().isBoss = true;
            m_MessageText.text = "BOSS BATTLE!";

            yield return m_BossIntroWait;
            m_MessageText.text = string.Empty;

            EnableTankControl();
        }

        while (!OneTankLeft() && m_Tanks[1].m_Instance.activeSelf)
        {
            yield return null;
        }
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();
        m_RealRoundWinner = GetRealRoundWinner();

        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
        }

        m_GameWinner = GetGameWinner();

        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private int GetLastEnemyIndex()
    {
        int numEnemyTanksLeft = 0;
        int LastEnemyIndex = -2;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf && m_Tanks[i].m_isCPU)
            {
                numEnemyTanksLeft++;
                LastEnemyIndex = i;
            }
        }

        if (numEnemyTanksLeft <= 1)
            return LastEnemyIndex;

        return -1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
            {
                return m_Tanks[i];
            }
        }

        return null;
    }

    private TankManager GetRealRoundWinner()
    {
        int numRoundWinners = 0;
        int index = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
            {
                numRoundWinners++;
                index = i;
            }
        }

        if (numRoundWinners == 1)
        {
            return m_Tanks[index];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        if (m_RoundWinner != null && m_RealRoundWinner == null)
            message = " THE CPUs WIN THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
            m_Tanks[i].m_Instance.GetComponent<TankMovement>().ResetSize();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}