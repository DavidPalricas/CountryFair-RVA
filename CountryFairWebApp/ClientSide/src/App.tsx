import {Canvas} from '@react-three/fiber'
import './App.css'
import zecaImg from './assets/imgs/Zeca.png'
import { connect } from './network/client'
import { useEffect } from 'react'

function App() {
   useEffect(() => {
    const roomPromise = connect()
    return () => {
      roomPromise.then((room) => room.leave())
    }
  }, [])

  return (
   <div className="App">
      <Canvas>
       {/* Game Stuff */}
      </Canvas>

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

export default App
