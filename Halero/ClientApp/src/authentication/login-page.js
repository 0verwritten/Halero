import React from 'react';
import cookies from "js-cookies";
import { toast } from 'react-toastify';

export class Logino extends React.Component{
    constructor(props){
        super(props);

        this.state = {
            username: "",
            password: ""
        }
    }
    handleInput(event){
        let target = event.target;
        let name = target.name;
        let value = target.value;

        this.setState({ [name]: value });
    }

    handleSubmit(e){
        e.preventDefault();
        const username = this.state.username;
        const password = this.state.password;
        // fetch("/api/authentication/login", { userName: username, password: password, method: "post" })
        //         .finally();


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
                    .then( responce => {
                        if(responce.token != null){
                            cookies.setItem("refreshToken", responce.token.refreshToken);
                            cookies.setItem("accessToken", responce.token.accessToken);
                            window.location.reload();
                        }else{
                            responce.errors.forEach( error => toast.error(error) );
                        }
                    });
    }

    render(){
        return (
            // <form className="logini" action="/api/authentication/login" method="GET">
            <form className="logini" onSubmit={e => this.handleSubmit(e)}>
                <div className="inputoBorder">
                    <input className="inputo" name="username" placeholder="Username or Email" type="text" required
                            onChange={ e => this.handleInput(e) }/>
                </div>
                <div className="inputoBorder">
                    <input className="inputo" name="password" placeholder="Password" type="password"  required
                            onChange={ e => this.handleInput(e) }/>
                </div>
                <input type="submit" className="result" value="Log in" />
            </form>
        );
    }
}