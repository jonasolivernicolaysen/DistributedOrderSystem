import { Link } from "react-router-dom";

function Navbar() {
    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
            <div className="container justify-content-center">

                <ul className="navbar-nav">
                    <li className="nav-item mx-2">
                        <Link className="nav-link" to="/register">
                            Register
                        </Link>
                    </li>

                    <li className="nav-item mx-2">
                        <Link className="nav-link" to="/login">
                            Login
                        </Link>
                    </li>

                    <li className="nav-item mx-2">
                        <Link className="nav-link" to="/products">
                            Products
                        </Link>
                    </li>

                    <li className="nav-item mx-2">
                        <Link className="nav-link" to="/cart">
                            Cart
                        </Link>
                    </li>

                    <li className="nav-item mx-2">
                        <Link className="nav-link" to="/profile">
                            Profile
                        </Link>
                    </li>
                </ul>

            </div>
        </nav>
    );
}

export default Navbar;