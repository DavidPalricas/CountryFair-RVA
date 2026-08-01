import zecaImg from '../assets/imgs/Zeca.png'
import '../App.css'

/**
 * Screen shown while the headset has not joined the room yet.
 *
 * Purely presentational — the transition out of it is driven by the `"gamejoined"` message
 * handled in `App`. Zeca Bigodes is the same host character that gives the intro dialogue
 * inside the VR game, so the patient and therapist see a consistent guide on both devices.
 */
export function WaitingScreen() {
   return (
     <div className="App">
        <div className="info">
            <div className="signboard">
            <h1 className="signboard__title">Bem vindo ao Country Fair VR</h1>

            <div className="signboard__body">
                <img src={zecaImg} alt="ZecaBigodes" />

                <div className="signboard__status">
                <h1>Esperando que ligue ao jogo</h1>

                <div className="waiting-dots" aria-hidden="true">
                    <span />
                    <span />
                    <span />
                </div>
                </div>
            </div>
            </div>
        </div>
    </div>
   )
}
