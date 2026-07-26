import zecaImg from '../assets/imgs/Zeca.png'
import '../App.css'

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