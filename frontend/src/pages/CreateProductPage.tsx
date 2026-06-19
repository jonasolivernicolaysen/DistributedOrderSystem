import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

function CreateProductPage() {
    // state

    const [productName, setProductName] = useState("");
    const [productDescription, setProductDescription] = useState("");
    const [productPrice, setProductPrice] = useState(0);

    const navigate = useNavigate();

    // functions
    const addProductListing = async () => {
        try {
            const token = localStorage.getItem("token");

            const response = await fetch(
                "https://localhost:7144/api/products",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        name: productName,
                        description: productDescription,
                        price: productPrice
                    })
                });
            console.log(JSON.stringify({
                name: productName,
                description: productDescription,
                price: productPrice
            }))

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                return;
            }
            alert("Product listing added successfully");
            navigate("/products")
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };

    // UI
    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-4">

                    <div className="card shadow">
                        <div className="card-body">

                            <h2 className="text-center mb-4">
                                Create Product
                            </h2>

                            <div className="mb-3">
                                <input
                                    className="form-control"
                                    value={productName}
                                    onChange={(e) => setProductName(e.target.value)}
                                    type="text"
                                    placeholder="Name"
                                />
                            </div>

                            <div className="mb-3">
                                <input
                                    className="form-control"
                                    value={productDescription}
                                    onChange={(e) => setProductDescription(e.target.value)}
                                    type="text"
                                    placeholder="Description"
                                />
                            </div>

                            <div className="mb-3">
                                <input
                                    className="form-control"
                                    value={productPrice}
                                    onChange={(e) => setProductPrice(Number(e.target.value))}
                                    type="number"
                                />
                            </div>

                            <button
                                className="btn btn-primary w-100"
                                onClick={() => {
                                    addProductListing();
                                }}
                            >
                                Add
                            </button>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    );
}

export default CreateProductPage;