import {Canvas} from '@react-three/fiber'
import './App.css'
import zecaImg from './assets/imgs/Zeca.png'

function App() {

  return (
   <div className="App">
      <Canvas>
       /* Game Stuff */
      </Canvas>

      <div className="info">
        <h1>Bem vindo ao Country Fair VR</h1>
        <img src={zecaImg} alt="ZecaBigodes" />

         <h1>Esperando que o se conecte ao jogo</h1>
      </div>
    </div>
  )
}

export default App
