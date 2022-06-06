import React from 'react';
import { Logino } from './login-page';
import { Registro } from './register-page';
import image from './imgs/icon.png';
import { ToastContainer } from 'react-toastify';


export class AuthenticationPage extends React.Component{
    constructor(props){
        super(props);

        this.state = {
            isLogin: true
        }
    }

    render(){
        require("./css/authentication-layout.css");
        return (
            <>
            <ToastContainer
                position="top-right"
                autoClose={5000}
                hideProgressBar
                newestOnTop={false}
                closeOnClick
                rtl={false}
                pauseOnFocusLoss
                draggable
                pauseOnHover
            />
            <div className="generalPlate">
                <div className="panel">
                    <div className="controls">
                        <button id="gologin"    onClick={(() => this.setState( { isLogin: true } ))}  disabled={this.state.isLogin}>Already a player?</button>
                        <button id="gosignup"   onClick={(() => this.setState( { isLogin: false } ))} disabled={!this.state.isLogin}>Haven't played?</button>
                    </div>
                    { (() => this.state.isLogin ? <Logino /> : <Registro />)() }
                </div>
                <img className="siteLogo" src={ image } alt="GameLogo" />
            </div>
            </>
        );
    }
}