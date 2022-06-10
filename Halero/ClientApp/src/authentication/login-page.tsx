import React from 'react';
import { setCookie } from "typescript-cookie";
import { toast } from 'react-toastify';
import { AuthenticationForm } from "../models/authentication-models";

type LoginStates = {username: string, password: string};
export class Logino extends React.Component<{}, LoginStates>{
    constructor(props: {}){
        super(props);

        this.state = {
            username: "",
            password: ""
        }
    }

    handleSubmit(e: React.FormEvent<HTMLFormElement>){
        e.preventDefault();
        const username = this.state.username;
        const password = this.state.password;


        fetch(`/api/authentication/Login`, { 
                    method: "POST", 
                    headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                    }, 
                    body: JSON.stringify({ 
                        userName: username, 
                        password: password
                    })
                }).then(res => res.json() )
                    .then( (responce: AuthenticationForm) => {
                        if(responce.token != null){
                            setCookie("refreshToken", responce.token!.refreshToken);
                            setCookie("accessToken", responce.token!.accessToken);
                            window.location.reload();
                        }else{
                            responce.errors!.forEach( error => toast.error(error) );
                        }
                    });
    }

    render(){
        return (
            // <form className="logini" action="/api/authentication/login" method="GET">
            <form className="logini" onSubmit={e => this.handleSubmit(e)}>
                <div className="inputoBorder">
                    <input className="inputo" name="username" placeholder="Username or Email" type="text" required
                            onChange={ e => this.setState({ username: e.target.value }) }/>
                </div>
                <div className="inputoBorder">
                    <input className="inputo" name="password" placeholder="Password" type="password"  required
                            onChange={ e => this.setState({ password: e.target.value }) }/>
                </div>
                <input type="submit" className="result" value="Log in" />
            </form>
        );
    }
}