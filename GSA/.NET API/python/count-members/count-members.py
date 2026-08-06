import clr
from argparse import ArgumentParser

clr.AddReference(r"C:\Program Files\Oasys\GSA 10.2\GsaAPI")

import GsaAPI  # noqa E402
from GsaAPI import MemberType  # noqa E402

if __name__ == "__main__":
    parser = ArgumentParser(description="Count the number of members in a GSA model")
    parser.add_argument("input_filename", help="Input GSA file")
    args = parser.parse_args()

    model = GsaAPI.Model(args.input_filename)
    print(f"Number of members: {len(model.Members())}")
    print(f"Number of elements: {len(model.Elements())}")
    print(f"Number of nodes: {len(model.Nodes())}")

    for member_id, member in model.Members().items():
        print(f"{member_id}: {member.Topology}")
        topo = [int(n) for n in member.Topology.split(" ")]
        print(f"  ({model.Nodes()[topo[0]].Position.X}, {model.Nodes()[topo[0]].Position.Y}, {model.Nodes()[topo[0]].Position.Z}) -> \
({model.Nodes()[topo[1]].Position.X}, {model.Nodes()[topo[1]].Position.Y}, {model.Nodes()[topo[1]].Position.Z})")